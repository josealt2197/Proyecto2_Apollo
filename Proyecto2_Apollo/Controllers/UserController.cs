using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Proyecto2_Apollo.Models;

namespace Proyecto2_Apollo.Controllers
{

    public class UserController : Controller
    {
        //Registration Action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }
        //Registration POST action 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")] User user)
        {
            bool Status = false;
            string message = "";

            // Model Validation 
            if (ModelState.IsValid)
            {

                #region //Email already exists 
                var isExistA = IsEmailExist(user.Email);
                if (isExistA)
                {
                    ModelState.AddModelError("EmailExist", "Ya existe una cuenta con el correo electrónico ingresado");
                    return View(user);
                }
                #endregion

                #region //ID already exists 
                var isExistB = IsIDExist(user.ID);
                if (isExistB)
                {
                    ModelState.AddModelError("IDExist", "Ya existe una cuenta con el la cédula ingresada");
                    return View(user);
                }
                #endregion

                #region Generate Activation Code 
                user.ActivationCode = Guid.NewGuid();
                #endregion

                #region  Password Hashing 
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword); //
                #endregion
                user.IsEmailVerified = false;

                #region Save to Database
                using (ApolloEntities dc = new ApolloEntities())
                {
                    dc.Users.Add(user);
                    dc.SaveChanges();

                    //Send Email to User
                    SendVerificationLinkEmail(user.Email, user.ActivationCode.ToString());
                    message = "Registro realizado correctamente. El enlace de activación de la cuenta" +
                        " ha sido enviado a su correo electrónico: " + user.Email;
                    Status = true;
                }
                #endregion
            }
            else
            {
                message = "Solicitud no válida";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }

        [HttpGet]
        public ActionResult RegisterQuestion(string user)
        {
            ViewBag.userid = user;
            return View();
        }

        //User Questions
        [HttpPost]
        public ActionResult RegisterQuestion(Question q)
        {
            bool Status = false;
            string message = "";
            try
            {
                q.AnswerOne = Crypto.Hash(q.AnswerOne);
                q.AnswerTwo = Crypto.Hash(q.AnswerTwo);
                q.AnswerThree = Crypto.Hash(q.AnswerThree);
                using (ApolloEntities dc = new ApolloEntities())
                {
                    //Question qu1 = new Question();
                    //qu1.Answers = Answers;
                    dc.Questions.Add(q);
                    dc.SaveChanges();

                    message = "Las preguntas han sido guardadas exitosamente.";
                    Status = true;
                }
            }
            catch (Exception e)
            {
                e.Message.ToString();
                message = "Solicitud no válida";
            }
            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(q);
        }


        //Verify Account  
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            string message = "";
            using (ApolloEntities dc = new ApolloEntities())
            {
                dc.Configuration.ValidateOnSaveEnabled = false; // This line I have added here to avoid
                                                                // Confirm password does not match issue on save changes
                var v = dc.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.IsEmailVerified = true;
                    dc.SaveChanges();
                    Status = true;
                    message = Convert.ToString(v.UserID);

                }
                else
                {
                    ViewBag.Message = "Solicitud no válida.";
                }
            }
            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View();
            //return RedirectToAction("RegisterQuestion", "User");
        }


        [HttpPost]
        public ActionResult VerifyAccount(string idusuario, string valor)
        {
            bool Status = false;
            string message = "";
            int idu = Convert.ToInt32(idusuario);
            //TempData["CreditCardInfo"] = idusuario;
            using (ApolloEntities dc = new ApolloEntities())
            {
                var v = dc.Users.Where(a => a.UserID == idu).FirstOrDefault();
                if (v != null)
                {
                    //string fNameStr = v.GetType().GetProperty("UserID").GetValue(v, null).ToString();
                    //u.UserID = Convert.ToInt32(fNameStr);
                    //dc.SaveChanges();
                    TempData["id"] = idusuario;
                    Status = true;
                    message = Convert.ToString(v.UserID);
                }
                else
                {
                    ViewBag.Message = "Solicitud no válida.";
                }

            }
            ViewBag.Message = message;
            ViewBag.Status = Status;
            return RedirectToAction("RegisterQuestion", "User", new { user = idu });
            //RegisterQuestion(idusuario);
        }



        //Login 
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        //Login POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin login, string ReturnUrl = "")
        {
            string message = "";
            using (ApolloEntities dc = new ApolloEntities())
            {
                var v = dc.Users.Where(a => a.Email == login.Email).FirstOrDefault();
                if (v != null)
                {
                    if (!v.IsEmailVerified)
                    {
                        ViewBag.Message = "Por favor verifique su correo electrónico antes de ingresar.";
                        return View();
                    }

                    if (string.Compare(Crypto.Hash(login.Password), v.Password) == 0)
                    {
                        int timeout = login.RememberMe ? 525600 : 20;
                        var ticket = new FormsAuthenticationTicket(login.Email, login.RememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);

                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            Random generator = new Random();
                            var codes = generator.Next(0, 999999).ToString("D6"); ;
                            v.Code = codes;
                            dc.Configuration.ValidateOnSaveEnabled = false;
                            dc.SaveChanges();
                            SendVerificationLinkEmail(v.Email, codes, "TwoStepCode");
                            return RedirectToAction("TwoStepCode", "User");
                        }
                    }
                    else if (v.IP != null)
                    {
                        return RedirectToAction("Bloqueo", "User");
                    }
                    else
                    {
                        message = "Alguna de sus credenciales no es correcta, intente de nuevo.";
                        v.Contador = v.Contador - 1;
                        dc.Configuration.ValidateOnSaveEnabled = false;
                        dc.SaveChanges();

                        if (v.Contador == 0)
                        {
                            var host = Dns.GetHostEntry(Dns.GetHostName());
                            foreach (var ip in host.AddressList)
                            {
                                v.IP = ip.ToString();
                                dc.Configuration.ValidateOnSaveEnabled = false;
                                dc.SaveChanges();
                                message = "Se ha bloqueado tu IP por sobrepasar el número de intentos";
                            }
                        }
                    }
                }
                else
                {
                    message = "Alguna de sus credenciales no es correcta, intente de nuevo.";
                }
            }
            ViewBag.Message = message;
            return View();
        }

        //Bloqueo 
        [HttpGet]
        public ActionResult Bloqueo()
        {
            //var message = "";
            return View();
        }

        //Desbloqueo
        [HttpGet]
        public ActionResult Desbloqueo(UserLogin login)
        {
            string message = "";
            using (ApolloEntities dc = new ApolloEntities())
            {
                var user = dc.Users.Where(a => a.ID == login.ID).FirstOrDefault();
                if (user != null)
                {
                    user.IP = null;
                    user.Contador = 3;
                    message = "¡Tu cuenta ha sido desbloqueada exitosamente! Te recomendamos cambiar la contraseña.";
                }
                else
                {
                    message = "Codigo incorrecto.";
                }
            }
            ViewBag.Message = message;
            return View();
        }


        //2 StepCode 
        [HttpGet]
        public ActionResult TwoStepCode()
        {
            string message = "Verifica el codigo que se envio a tu correo";
            return View();
        }

        //2 StepCode
        [HttpPost]
        public ActionResult TwoStepCode(TwoStepCodeModel model, string ReturnUrl = "")
        {
            string message = "";
            using (ApolloEntities dc = new ApolloEntities())
            {
                var user = dc.Users.Where(a => a.Code == model.codes).FirstOrDefault();
                if (user != null)
                {
                    if (Url.IsLocalUrl(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    message = "Codigo incorrecto.";
                }
            }
            ViewBag.Message = message;
            return View();
        }


        //Logout
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }


        [NonAction]
        public bool IsEmailExist(string email)
        {
            using (ApolloEntities dc = new ApolloEntities())
            {
                var v = dc.Users.Where(a => a.Email == email).FirstOrDefault();
                return v != null;
            }
        }

        [NonAction]
        public bool IsIDExist(string idUser)
        {
            using (ApolloEntities dc = new ApolloEntities())
            {
                var v = dc.Users.Where(a => a.ID == idUser).FirstOrDefault();
                return v != null;
            }
        }

        [NonAction]
        public void SendVerificationLinkEmail(string emailID, string activationCode, string emailFor = "VerifyAccount")
        {
            var activateUrl = "/User/" + emailFor + "/" + activationCode;
            var activatelink = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, activateUrl);
            var verifyUrl = "/User/" + emailFor + "/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("prograVcuc@gmail.com", "Apollo Transactions");
            var toEmail = new MailAddress(emailID);
            var fromEmailPassword = "cursoCUC2019";

            string subject = "";
            string body = "";


            if (emailFor == "VerifyAccount")
            {
                subject = "¡Su cuenta ha sido creada exitosamente!";
                body = "<div style='background-color: #263238; padding:20px'>" +
                            "<div class='logo-right' align='right' id='emb-email-header'><img style = 'display: block;height: auto;width: 100%;border: 0;max-width: 227px;' src='https://i1.createsend1.com/ei/t/0E/821/C3B/075144/csfinal/Logo4.png' alt='' width='227'></div>" +
                            "<div style = 'mso-line-height-rule: exactly;mso-text-raise: 4px;'>" +
                                "<p class='size-40' style='Margin-top: 0;Margin-bottom: 20px;font-family: oswald,avenir next condensed,arial narrow,ms ui gothic,sans-serif;font-size: 32px;line-height: 40px;text-align: center;' lang='x-size-40'><span class='font-oswald'><strong><span style = 'color:#ffffff'> &#161;¡ Bienvenido a Apollo !</span></strong></span></p>" +
                            "</div>" +
                            "<div class='divider' style='display: block;font-size: 2px;line-height: 2px;Margin-left: auto;Margin-right: auto;width: 40px;background-color: #ccc;Margin-bottom: 20px;'>&nbsp;</div>" +
                            "<div style = 'Margin-left: 20px;Margin-right: 20px;' align='center'>" +
                                    "<img style = 'border: 0;display: block;height: auto;width: 100%;max-width: 128px;' alt='' width='128' src='https://i1.createsend1.com/ei/t/0E/821/C3B/140045/csfinal/question-990a28028a01453c.png'>" +
                            "</div>" +
                            "<div style = 'Margin-left: 20px;Margin-right: 20px;' >" +
                                    "<p class='size-22' style='Margin-top: 0;Margin-bottom: 0;font-family: montserrat,dejavu sans,verdana,sans-serif;font-size: 18px;line-height: 26px;text-align: center;' lang='x-size-22'><span class='font-montserrat'><span style = 'color:#ffffff' > Nos complace informarle que su cuenta de Apollo Transactions se ha creado correctamente  <br/ Por favor haga clic en el enlace de abajo para verificar su cuenta.></span></span></p>" +
                            "</div>" +
                            "<div style = 'Margin-left: 20px;Margin-right: 20px;' >" +
                            "</div>" +
                            "<div style = 'Margin-left: 20px;Margin-right: 20px;' >" +
                                    "<h2 style = 'Margin-top: 0;Margin-bottom: 16px;font-style: normal;font-weight: normal;color: #e31212;font-size: 20px;line-height: 34px;font-family: montserrat,dejavu sans,verdana,sans-serif;text-align: center;' ><span style = 'color:#fff' > <a style='color:#00c853' href='" + link + "'> Activar cuenta </a> </ span ></ span ></ h2 >" +
                            "</ div >" +
                        "</ div > ";

            }
            else if (emailFor == "ResetPassword")
            {
                subject = "Recuperar Contraseña";
                body = "<div style='background-color: #263238; padding:20px'>" +
                            "<div class='logo-right' align='right' id='emb-email-header'><img style = 'display: block;height: auto;width: 100%;border: 0;max-width: 227px;' src='https://i1.createsend1.com/ei/t/0E/821/C3B/075144/csfinal/Logo4.png' alt='' width='227'></div>" +
                            "<div style = 'mso-line-height-rule: exactly;mso-text-raise: 4px;'>" +
                                "<p class='size-40' style='Margin-top: 0;Margin-bottom: 20px;font-family: oswald,avenir next condensed,arial narrow,ms ui gothic,sans-serif;font-size: 32px;line-height: 40px;text-align: center;' lang='x-size-40'><span class='font-oswald'><strong><span style = 'color:#ffffff'> &#161;Hola!</span></strong></span></p>" +
                            "</div>" +
                            "<div class='divider' style='display: block;font-size: 2px;line-height: 2px;Margin-left: auto;Margin-right: auto;width: 40px;background-color: #ccc;Margin-bottom: 20px;'>&nbsp;</div>" +
                            "<div style = 'Margin-left: 20px;Margin-right: 20px;' align='center'>" +
                                    "<img style = 'border: 0;display: block;height: auto;width: 100%;max-width: 128px;' alt='' width='128' src='https://i1.createsend1.com/ei/t/0E/821/C3B/140045/csfinal/question-990a28028a01453c.png'>" +
                            "</div>" +
                            "<div style = 'Margin-left: 20px;Margin-right: 20px;' >" +
                                    "<p class='size-22' style='Margin-top: 0;Margin-bottom: 0;font-family: montserrat,dejavu sans,verdana,sans-serif;font-size: 18px;line-height: 26px;text-align: center;' lang='x-size-22'><span class='font-montserrat'><span style = 'color:#ffffff' > Tenemos solicitud para restablecer la contraseña de su cuenta. Haga clic en el enlace de abajo para restablecer su contraseña </span></span></p>" +
                            "</div>" +
                            "<div style = 'Margin-left: 20px;Margin-right: 20px;' >" +
                            "</div>" +
                            "<div style = 'Margin-left: 20px;Margin-right: 20px;' >" +
                                    "<h2 style = 'Margin-top: 0;Margin-bottom: 16px;font-style: normal;font-weight: normal;color: #e31212;font-size: 20px;line-height: 34px;font-family: montserrat,dejavu sans,verdana,sans-serif;text-align: center;' ><span style = 'color:#fff' > <a style='color:#00c853' href=" + link + ">Restaurar contraseña</a> </ span ></ span ></ h2 >" +
                            "</ div >" +
                        "</ div > ";
            }
            else if (emailFor == "TwoStepCode")
            {
                subject = "Codigo de Acceso";
                body = "<div style='background-color: #263238; padding:20px'>" +
                            "<div class='logo-right' align='right' id='emb-email-header'><img style = 'display: block;height: auto;width: 100%;border: 0;max-width: 227px;' src='https://i1.createsend1.com/ei/t/0E/821/C3B/075144/csfinal/Logo4.png' alt='' width='227'></div>" +
                            "<div style = 'mso-line-height-rule: exactly;mso-text-raise: 4px;'>" +
                                "<p class='size-40' style='Margin-top: 0;Margin-bottom: 20px;font-family: oswald,avenir next condensed,arial narrow,ms ui gothic,sans-serif;font-size: 32px;line-height: 40px;text-align: center;' lang='x-size-40'><span class='font-oswald'><strong><span style = 'color:#ffffff'> &#161;¡Hola!</span></strong></span></p>" +
                            "</div>" +
                            "<div class='divider' style='display: block;font-size: 2px;line-height: 2px;Margin-left: auto;Margin-right: auto;width: 40px;background-color: #ccc;Margin-bottom: 20px;'>&nbsp;</div>" +
                            "<div style = 'Margin-left: 20px;Margin-right: 20px;' align='center'>" +
                                    "<img style = 'border: 0;display: block;height: auto;width: 100%;max-width: 128px;' alt='' width='128' src='https://i1.createsend1.com/ei/t/0E/821/C3B/140045/csfinal/question-990a28028a01453c.png'>" +
                            "</div>" +
                            "<div style = 'Margin-left: 20px;Margin-right: 20px;' >" +
                                    "<p class='size-22' style='Margin-top: 0;Margin-bottom: 0;font-family: montserrat,dejavu sans,verdana,sans-serif;font-size: 18px;line-height: 26px;text-align: center;' lang='x-size-22'><span class='font-montserrat'><span style = 'color:#ffffff' > Hemos recibido una solicitud de ingreso a tu cuenta.</span></span></p><p class='size-22' style='Margin-top: 20px;Margin-bottom: 20px;font-family: montserrat,dejavu sans,verdana,sans-serif;font-size: 18px;line-height: 26px;text-align: center;' lang='x-size-22'><span class='font-montserrat'><span style = 'color:#ffffff' > Tu código para ingresar es :</span></span></p>" +
                            "</div>" +
                            "<div style = 'Margin-left: 20px;Margin-right: 20px;' >" +
                            "</div>" +
                            "<div style = 'Margin-left: 20px;Margin-right: 20px;' >" +
                                    "<h2 style = 'Margin-top: 0;Margin-bottom: 16px;font-style: normal;font-weight: normal;color: #e31212;font-size: 26px;line-height: 34px;font-family: montserrat,dejavu sans,verdana,sans-serif;text-align: center;' ><span style = 'color:#00c853' > " + activationCode + " </ span ></ span ></ h2 >" +
                            "</ div >" +
                        "</ div > ";
            }
            else if (emailFor == "Desbloqueo")
            {
                subject = "Recuperar Acceso";
                body = "<div style='background-color: #263238; padding:20px'>" +
                            "<div class='logo-right' align='right' id='emb-email-header'><img style = 'display: block;height: auto;width: 100%;border: 0;max-width: 227px;' src='https://i1.createsend1.com/ei/t/0E/821/C3B/075144/csfinal/Logo4.png' alt='' width='227'></div>" +
                            "<div style = 'mso-line-height-rule: exactly;mso-text-raise: 4px;'>" +
                                "<p class='size-40' style='Margin-top: 0;Margin-bottom: 20px;font-family: oswald,avenir next condensed,arial narrow,ms ui gothic,sans-serif;font-size: 32px;line-height: 40px;text-align: center;' lang='x-size-40'><span class='font-oswald'><strong><span style = 'color:#ffffff'> &#161;Hola!</span></strong></span></p>" +
                            "</div>" +
                            "<div class='divider' style='display: block;font-size: 2px;line-height: 2px;Margin-left: auto;Margin-right: auto;width: 40px;background-color: #ccc;Margin-bottom: 20px;'>&nbsp;</div>" +
                            "<div style = 'Margin-left: 20px;Margin-right: 20px;' align='center'>" +
                                    "<img style = 'border: 0;display: block;height: auto;width: 100%;max-width: 128px;' alt='' width='128' src='https://i1.createsend1.com/ei/t/0E/821/C3B/140045/csfinal/question-990a28028a01453c.png'>" +
                            "</div>" +
                            "<div style = 'Margin-left: 20px;Margin-right: 20px;' >" +
                                    "<p class='size-22' style='Margin-top: 0;Margin-bottom: 0;font-family: montserrat,dejavu sans,verdana,sans-serif;font-size: 18px;line-height: 26px;text-align: center;' lang='x-size-22'><span class='font-montserrat'><span style = 'color:#ffffff' > Alguien estuvo intentando acceder a tu cuenta por lo que la hemos bloqueado.</span></span></p><p class='size-22' style='Margin-top: 20px;Margin-bottom: 20px;font-family: montserrat,dejavu sans,verdana,sans-serif;font-size: 18px;line-height: 26px;text-align: center;' lang='x-size-22'><span class='font-montserrat'><span style = 'color:#ffffff' > Accede al link para desbloquearla:</span></span></p>" +
                            "</div>" +
                            "<div style = 'Margin-left: 20px;Margin-right: 20px;' >" +
                            "</div>" +
                            "<div style = 'Margin-left: 20px;Margin-right: 20px;' >" +
                                    "<h2 style = 'Margin-top: 0;Margin-bottom: 16px;font-style: normal;font-weight: normal;color: #e31212;font-size: 26px;line-height: 34px;font-family: montserrat,dejavu sans,verdana,sans-serif;text-align: center;' ><span style = 'color:#00c853' > <a style='color:#00c853' href=" + activatelink + "> Recuperar acceso </a> </ span ></ span ></ h2 >" +
                            "</ div >" +
                        "</ div > ";
            }


            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
        }


        [HttpGet]
        public ActionResult RespondQuestion(string user)
        {
            bool Status = false;
            string message = "";
            int uid = 0;

            using (ApolloEntities dc = new ApolloEntities())
            {
                var v = dc.Users.Where(a => a.Email == user).FirstOrDefault();
                if (v != null)
                {
                    uid = v.UserID;
                    ViewBag.userid = v.UserID;
                    var w = dc.Questions.Where(b => b.FUserID == uid).FirstOrDefault();
                    if (w != null)
                    {
                        ViewBag.Q1 = w.UserQuestionOne;
                        ViewBag.Q2 = w.UserQuestionTwo;
                        ViewBag.Q3 = w.UserQuestionThree;

                    }
                }
                else
                {
                    message = "No se han encontrado Preguntas de Seguridad asociadas a esta cuenta.";
                    Status = true;
                }

            }

            ViewBag.Status = Status;
            ViewBag.Message = message;
            return View();
        }

        [HttpPost]
        public ActionResult RespondQuestion(Question model)
        {
            bool Status = false;
            string message = "";
            bool right = false;

            using (ApolloEntities dc = new ApolloEntities())
            {
                var v = dc.Questions.Where(a => a.FUserID == model.FUserID).FirstOrDefault();
                if (v != null)
                {
                    if (model.UserQuestionOne == v.UserQuestionOne) {
                        if (string.Compare(Crypto.Hash(model.AnswerOne), v.AnswerOne) == 0)
                        {
                            right = true;
                        }  
                    }

                    if (model.UserQuestionOne == v.UserQuestionTwo) {
                        if (string.Compare(Crypto.Hash(model.AnswerOne), v.AnswerTwo) == 0)
                        {
                            right = true;
                        }
                    }

                    if (model.UserQuestionOne == v.UserQuestionThree){
                        if (string.Compare(Crypto.Hash(model.AnswerOne), v.AnswerThree) == 0)
                        {
                            right = true;
                        }
                    }

                    if (right == true)
                    {

                        var w = dc.Users.Where(b => b.UserID == model.FUserID).FirstOrDefault();
                        if (w != null)
                        {
                            //Send email for reset password
                            string resetCode = Guid.NewGuid().ToString();
                            SendVerificationLinkEmail(w.Email, resetCode, "ResetPassword");
                            w.ResetPasswordCode = resetCode;
                            dc.Configuration.ValidateOnSaveEnabled = false;
                            dc.SaveChanges();
                            message = "El enlace para restablecer la contraseña ha sido enviado a su correo electrónico.";
                            Status = true;
                        }
                        else
                        {
                            message = "No ha sido posible enviar las instrucciones para reestaurar la contraseña.";
                            Status = true;
                        }

                    }
                    
                }
                else
                {
                    message = "No se han encontrado Preguntas de Seguridad asociadas a esta cuenta.";
                    Status = true;
                }
            }
            ViewBag.Status = Status;
            ViewBag.Message = message;
            return View();
        }


        //Forgot Password

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string Email)
        {
            //Verify Email ID
            //Generate Reset password link 
            //Send Email 
            string message = "";
            //bool status = false;

            using (ApolloEntities dc = new ApolloEntities())
            {
                var account = dc.Users.Where(a => a.Email == Email).FirstOrDefault();
                if (account != null)
                {
                   return RedirectToAction("RespondQuestion", "User", new { user = Email });
                }
                else
                {
                    message = "Cuenta no encontrada.";
                }
            }
            ViewBag.Message = message;
            return View();
            
        }

        public ActionResult ResetPassword(string id)
        {
            //Verify the reset password link
            //Find account associated with this link
            //redirect to reset password page
            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();
            }

            using (ApolloEntities dc = new ApolloEntities())
            {
                var user = dc.Users.Where(a => a.ResetPasswordCode == id).FirstOrDefault();
                if (user != null)
                {
                    ResetPasswordModel model = new ResetPasswordModel();
                    model.ResetCode = id;
                    return View(model);
                }
                else
                {
                    return HttpNotFound();
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            var message = "";
            if (ModelState.IsValid)
            {
                using (ApolloEntities dc = new ApolloEntities())
                {
                    var user = dc.Users.Where(a => a.ResetPasswordCode == model.ResetCode).FirstOrDefault();
                    if (user != null)
                    {
                        user.Password = Crypto.Hash(model.NewPassword);
                        user.ResetPasswordCode = "";
                        dc.Configuration.ValidateOnSaveEnabled = false;
                        dc.SaveChanges();
                        message = "Nueva contraseña actualizada correctamente.";
                    }
                }
            }
            else
            {
                message = "Se ha producido un error al actualizar su contraseña.";
            }
            ViewBag.Message = message;
            return View(model);
        }
    }

}