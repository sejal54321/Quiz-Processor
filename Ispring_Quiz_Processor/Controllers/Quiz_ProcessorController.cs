using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Xml.Linq;
using System.Net;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Xml;
using Ispring_Quiz_Processor.Model;
using Ispring_Quiz_Processor.Data;
using Microsoft.AspNetCore.Hosting;
using Ispring_Quiz_Processor.Services;


namespace Ispring_Quiz_Processor.Controllers
{
    [Route("api/QuizAPI")]
    [ApiController]

    public class Quiz_ProcessorController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly EmailSender _emailSender;

        public Quiz_ProcessorController(ApplicationDbContext context, EmailSender EmailSender)
        {
            _context = context;
            _emailSender = EmailSender;

        }
        public class RawXml
        {
            public string RazorXml { get; set; }
        }
        [HttpPost("GetData")]
        public async Task<IActionResult> GetData()
        {
            SummaryDetails quiz = new SummaryDetails();

            XmlDetails xmldtl = new XmlDetails();
            XmlDocument doc = new XmlDocument();


            using (var transaction = _context.Database.BeginTransaction())
            {
                string Qtytype = "";
                string XmlContent = "";
                try
                {

                    XmlContent = Request.Form["dr"];
                    quiz.UserName = Request.Form["USER_NAME"];
                    quiz.UserEmail = Request.Form["USER_EMAIL"];

                    if (XmlContent != "")
                    {

                        doc.LoadXml(XmlContent);
                        XmlNodeList NodeList = doc.GetElementsByTagName("quizReport");

                        if (NodeList.Count > 0 && NodeList != null)
                        {
                            List<QuestionDetails> QuestionDetail = new List<QuestionDetails>();
                            foreach (XmlNode node in NodeList)
                            {
                                if (node.ChildNodes != null && node.ChildNodes.Count > 0)
                                {
                                    quiz.Score = Convert.ToInt32(Request.Form["tp"]);
                                    quiz.PassingPercent = Convert.ToDecimal(Request.Form["psp"]);
                                    quiz.PassingScore = Convert.ToDecimal(Request.Form["ps"]);
                                    quiz.IsPassed = Convert.ToBoolean(node.ChildNodes[1].Attributes["passed"].Value);
                                    quiz.TimeTaken = Convert.ToInt32(node.ChildNodes[1].Attributes["time"].Value);
                                    quiz.Date = Convert.ToDateTime(node.ChildNodes[1].Attributes["finishTimestamp"].Value);
                                    if(node.ChildNodes[3].ChildNodes.Count > 0)
                                    {
                                        quiz.TotalQuestion = Convert.ToInt32(node.ChildNodes[3].ChildNodes[0].Attributes["totalQuestions"].Value);
                                        quiz.AnsweredQuestion = Convert.ToInt32(node.ChildNodes[3].ChildNodes[0].Attributes["answeredQuestions"].Value);

                                    }
                                    else
                                    {
                                        quiz.TotalQuestion = 0;
                                        quiz.AnsweredQuestion = 0;
                                    }
                                    quiz.Percentage = Convert.ToDecimal(node.ChildNodes[1].Attributes["percent"].Value);
                                    quiz.CreatedDate = DateTime.Now;

                                    _context.SummaryDetails.Add(quiz);
                                    _context.SaveChanges();


                                    xmldtl.SummaryDetailId = (quiz.Id == null ? 0 : quiz.Id);
                                    xmldtl.XmlDetail = Convert.ToString(XmlContent);
                                    xmldtl.CreatedDate = DateTime.Now;

                                    _context.XmlDetails.Add(xmldtl);
                                    _context.SaveChanges();


                                    if (node.ChildNodes[2].ChildNodes.Count > 0)
                                    {

                                        foreach (XmlNode Qtnlst in node.ChildNodes[2].ChildNodes)
                                        {
                                            Qtytype = "";
                                            if (Qtnlst.LocalName == "multipleChoiceQuestion")
                                            {

                                                QuestionDetails que = new QuestionDetails();
                                                Qtytype = Qtnlst.LocalName;
                                                que.QuizQuestion = Qtnlst.ChildNodes[0].InnerText;
                                                que.QuestionType = Qtnlst.LocalName;
                                                que.AwardedPoints = Convert.ToInt32(Qtnlst.Attributes["awardedPoints"].Value);
                                                que.MaxPoints = Convert.ToInt32(Qtnlst.Attributes["maxPoints"].Value);


                                                int crtansid = Convert.ToInt32(Qtnlst.ChildNodes[2].Attributes["correctAnswerIndex"].Value);
                                                int useransid = Convert.ToInt32(Qtnlst.ChildNodes[2].Attributes["userAnswerIndex"].Value);

                                                que.CorrectAnswer = Convert.ToString(Qtnlst.ChildNodes[2].ChildNodes[crtansid].InnerText);
                                                que.UserSelection = Convert.ToString(Qtnlst.ChildNodes[2].ChildNodes[useransid].InnerText);

                                                QuestionDetail.Add(que);

                                            }
                                            else if (Qtnlst.LocalName == "multipleResponseQuestion")
                                            {

                                                QuestionDetails que = new QuestionDetails();
                                                Qtytype = Qtnlst.LocalName;
                                                que.QuestionType = Qtnlst.LocalName;
                                                que.AwardedPoints = Convert.ToInt32(Qtnlst.Attributes["awardedPoints"].Value);
                                                que.MaxPoints = Convert.ToInt32(Qtnlst.Attributes["maxPoints"].Value);
                                                que.QuizQuestion = Qtnlst.ChildNodes[0].InnerText;

                                                string crtans = "", uselect = "";
                                                foreach (XmlNode item in Qtnlst.ChildNodes[2])
                                                {
                                                    bool crtansid = Convert.ToBoolean(item.Attributes["correct"].Value);
                                                    bool useransid = Convert.ToBoolean(item.Attributes["selected"].Value);

                                                    if (crtansid == true)
                                                    {
                                                        crtans += item.InnerText + ",";

                                                    }
                                                    if (useransid == true)
                                                    {
                                                        uselect += item.InnerText + ",";
                                                    }

                                                }
                                                crtans = crtans.Trim(',');
                                                uselect = uselect.Trim(',');
                                                que.CorrectAnswer = crtans;
                                                que.UserSelection = uselect;

                                                QuestionDetail.Add(que);

                                            }
                                            else if (Qtnlst.LocalName == "trueFalseQuestion")
                                            {
                                                QuestionDetails que = new QuestionDetails();
                                                Qtytype = Qtnlst.LocalName;
                                                que.QuestionType = Qtnlst.LocalName;
                                                que.AwardedPoints = Convert.ToInt32(Qtnlst.Attributes["awardedPoints"].Value);
                                                que.MaxPoints = Convert.ToInt32(Qtnlst.Attributes["maxPoints"].Value);
                                                que.QuizQuestion = Qtnlst.ChildNodes[0].InnerText;

                                                int crtansid = Convert.ToInt32(Qtnlst.ChildNodes[2].Attributes["correctAnswerIndex"].Value);
                                                int useransid = Convert.ToInt32(Qtnlst.ChildNodes[2].Attributes["userAnswerIndex"].Value);

                                                que.CorrectAnswer = Convert.ToString(Qtnlst.ChildNodes[2].ChildNodes[crtansid].InnerText);
                                                que.UserSelection = Convert.ToString(Qtnlst.ChildNodes[2].ChildNodes[useransid].InnerText);

                                                QuestionDetail.Add(que);

                                            }
                                            else if (Qtnlst.LocalName == "typeInQuestion")
                                            {
                                                QuestionDetails que = new QuestionDetails();
                                                Qtytype = Qtnlst.LocalName;
                                                que.QuestionType = Qtnlst.LocalName;
                                                que.AwardedPoints = Convert.ToInt32(Qtnlst.Attributes["awardedPoints"].Value);
                                                que.MaxPoints = Convert.ToInt32(Qtnlst.Attributes["maxPoints"].Value);
                                                que.QuizQuestion = Qtnlst.ChildNodes[0].InnerText;

                                                que.UserSelection = Convert.ToString(Qtnlst.Attributes["userAnswer"].Value);
                                                string accpAns = "";
                                                foreach (XmlNode item in Qtnlst.ChildNodes[2])
                                                {
                                                    accpAns += item.InnerText + ",";

                                                }
                                                accpAns = accpAns.Trim(',');
                                                que.CorrectAnswer = accpAns;

                                                QuestionDetail.Add(que);

                                            }
                                            else if (Qtnlst.LocalName == "numericQuestion")
                                            {
                                                QuestionDetails que = new QuestionDetails();
                                                Qtytype = Qtnlst.LocalName;
                                                que.QuestionType = Qtnlst.LocalName;
                                                que.AwardedPoints = Convert.ToInt32(Qtnlst.Attributes["awardedPoints"].Value);
                                                que.MaxPoints = Convert.ToInt32(Qtnlst.Attributes["maxPoints"].Value);
                                                que.QuizQuestion = Qtnlst.ChildNodes[0].InnerText;

                                                que.UserSelection = Convert.ToString(Qtnlst.Attributes["userAnswer"].Value);
                                                string accpAns = "";
                                                foreach (XmlNode item in Qtnlst.ChildNodes[2])
                                                {
                                                    accpAns += item.InnerText + ",";

                                                }
                                                accpAns = accpAns.Trim(',');
                                                que.CorrectAnswer = accpAns;

                                                QuestionDetail.Add(que);

                                            }
                                            else if (Qtnlst.LocalName == "sequenceQuestion")
                                            {
                                                QuestionDetails que = new QuestionDetails();
                                                Qtytype = Qtnlst.LocalName;
                                                que.QuestionType = Qtnlst.LocalName;
                                                que.AwardedPoints = Convert.ToInt32(Qtnlst.Attributes["awardedPoints"].Value);
                                                que.MaxPoints = Convert.ToInt32(Qtnlst.Attributes["maxPoints"].Value);
                                                que.QuizQuestion = Qtnlst.ChildNodes[0].InnerText;

                                                Dictionary<int, string> crtans = new Dictionary<int, string>();
                                                string uselect = "";
                                                string correctAns = "";
                                                foreach (XmlNode item in Qtnlst.ChildNodes[2])
                                                {
                                                    uselect += item.InnerText + ",";
                                                    int i = Convert.ToInt32(item.Attributes["originalIndex"].Value);
                                                    string value = Convert.ToString(item.InnerText);
                                                    crtans.Add(i, value);

                                                }
                                                if (crtans.Count > 0 && crtans != null)
                                                {
                                                    foreach (var i in crtans.OrderBy(m => m.Key))
                                                    {
                                                        correctAns += i.Value + ",";

                                                    }
                                                }

                                                correctAns = correctAns.Trim(',');
                                                uselect = uselect.Trim(',');
                                                que.UserSelection = uselect;
                                                que.CorrectAnswer = correctAns;

                                                QuestionDetail.Add(que);
                                            }
                                            else if (Qtnlst.LocalName == "matchingQuestion")
                                            {

                                                QuestionDetails que = new QuestionDetails();
                                                Qtytype = Qtnlst.LocalName;
                                                que.QuestionType = Qtnlst.LocalName;
                                                que.AwardedPoints = Convert.ToInt32(Qtnlst.Attributes["awardedPoints"].Value);
                                                que.MaxPoints = Convert.ToInt32(Qtnlst.Attributes["maxPoints"].Value);
                                                que.QuizQuestion = Qtnlst.ChildNodes[0].InnerText;


                                                string uselect = "";
                                                string correctAns = "";
                                                foreach (XmlNode item in Qtnlst.ChildNodes[4].ChildNodes)
                                                {
                                                    int perid = Convert.ToInt32(item.Attributes["premiseIndex"].Value);
                                                    int repid = Convert.ToInt32(item.Attributes["responseIndex"].Value);

                                                    string pertext = Convert.ToString(Qtnlst.ChildNodes[2].ChildNodes[perid].InnerText);
                                                    string matchtext = Convert.ToString(Qtnlst.ChildNodes[3].ChildNodes[repid].InnerText);


                                                    correctAns += (pertext + " - " + matchtext) + ",";

                                                }
                                                foreach (XmlNode item in Qtnlst.ChildNodes[5].ChildNodes)
                                                {
                                                    int perid = Convert.ToInt32(item.Attributes["premiseIndex"].Value);
                                                    int repid = Convert.ToInt32(item.Attributes["responseIndex"].Value);

                                                    string pertext = Convert.ToString(Qtnlst.ChildNodes[2].ChildNodes[perid].InnerText);
                                                    string matchtext = Convert.ToString(Qtnlst.ChildNodes[3].ChildNodes[repid].InnerText);


                                                    uselect += (pertext + " - " + matchtext) + ",";

                                                }
                                                correctAns = correctAns.Trim(',');
                                                uselect = uselect.Trim(',');
                                                que.CorrectAnswer = correctAns;
                                                que.UserSelection = uselect;

                                                QuestionDetail.Add(que);
                                            }
                                            else if (Qtnlst.LocalName == "fillInTheBlankQuestion")
                                            {
                                                QuestionDetails que = new QuestionDetails();
                                                Qtytype = Qtnlst.LocalName;
                                                que.QuestionType = Qtnlst.LocalName;
                                                que.AwardedPoints = Convert.ToInt32(Qtnlst.Attributes["awardedPoints"].Value);
                                                que.MaxPoints = Convert.ToInt32(Qtnlst.Attributes["maxPoints"].Value);
                                                que.QuizQuestion = Qtnlst.ChildNodes[0].InnerText;

                                                que.UserSelection = Convert.ToString(Qtnlst.ChildNodes[2].ChildNodes[1].Attributes["userAnswer"].Value);

                                                string correctAns = "";
                                                foreach (XmlNode item in Qtnlst.ChildNodes[2].ChildNodes[1].ChildNodes)
                                                {
                                                    correctAns += item.InnerText + ",";

                                                }
                                                correctAns = correctAns.Trim(',');
                                                que.CorrectAnswer = correctAns;

                                                QuestionDetail.Add(que);
                                            }
                                            else if (Qtnlst.LocalName == "multipleChoiceTextQuestion")
                                            {
                                                QuestionDetails que = new QuestionDetails();
                                                Qtytype = Qtnlst.LocalName;
                                                que.QuestionType = Qtnlst.LocalName;
                                                que.AwardedPoints = Convert.ToInt32(Qtnlst.Attributes["awardedPoints"].Value);
                                                que.MaxPoints = Convert.ToInt32(Qtnlst.Attributes["maxPoints"].Value);
                                                que.QuizQuestion = Qtnlst.ChildNodes[0].InnerText;


                                                int uid = Convert.ToInt32(Qtnlst.ChildNodes[2].ChildNodes[1].Attributes["userAnswerIndex"].Value);
                                                int cid = Convert.ToInt32(Qtnlst.ChildNodes[2].ChildNodes[1].Attributes["correctAnswerIndex"].Value);

                                                que.UserSelection = Convert.ToString(Qtnlst.ChildNodes[2].ChildNodes[1].ChildNodes[uid].InnerText);
                                                que.CorrectAnswer = Convert.ToString(Qtnlst.ChildNodes[2].ChildNodes[1].ChildNodes[cid].InnerText);

                                                QuestionDetail.Add(que);

                                            }
                                            else if (Qtnlst.LocalName == "wordBankQuestion")
                                            {
                                                QuestionDetails que = new QuestionDetails();
                                                Qtytype = Qtnlst.LocalName;
                                                que.QuestionType = Qtnlst.LocalName;
                                                que.AwardedPoints = Convert.ToInt32(Qtnlst.Attributes["awardedPoints"].Value);
                                                que.MaxPoints = Convert.ToInt32(Qtnlst.Attributes["maxPoints"].Value);
                                                que.QuizQuestion = Qtnlst.ChildNodes[0].InnerText;
                                                string correctAns = ""; string uselect = "";
                                                foreach (XmlNode item in Qtnlst.ChildNodes[2].ChildNodes)
                                                {
                                                    if (item.LocalName == "blank")
                                                    {
                                                        correctAns += item.Attributes["userAnswer"].Value + ",";
                                                        uselect += item.InnerText + ",";
                                                    }
                                                }
                                                correctAns = correctAns.Trim(',');
                                                uselect = uselect.Trim(',');
                                                que.CorrectAnswer = correctAns;
                                                que.UserSelection = uselect;

                                                QuestionDetail.Add(que);
                                            }
                                            else if (Qtnlst.LocalName == "hotspotQuestion")
                                            {
                                                QuestionDetails que = new QuestionDetails();
                                                Qtytype = Qtnlst.LocalName;
                                                que.QuestionType = Qtnlst.LocalName;
                                                que.AwardedPoints = Convert.ToInt32(Qtnlst.Attributes["awardedPoints"].Value);
                                                que.MaxPoints = Convert.ToInt32(Qtnlst.Attributes["maxPoints"].Value);
                                                que.QuizQuestion = Qtnlst.ChildNodes[0].InnerText;

                                                //string userx = Convert.ToString(Qtnlst.ChildNodes[2].ChildNodes[0].Attributes["x"].Value);
                                                //string usery = Convert.ToString(Qtnlst.ChildNodes[2].ChildNodes[0].Attributes["y"].Value);


                                                string uselect = "";
                                                foreach (XmlNode item in Qtnlst.ChildNodes[2].ChildNodes)
                                                {                                                  
                                                  uselect += "x=" + item.Attributes["x"].Value + " y=" + item.Attributes["y"].Value + ",";                                                      
                                                    
                                                }                                           

                                                string corx = Convert.ToString(Qtnlst.ChildNodes[3].ChildNodes[0].Attributes["x"].Value);
                                                string cory = Convert.ToString(Qtnlst.ChildNodes[3].ChildNodes[0].Attributes["y"].Value);
                                                bool ismarked = Convert.ToBoolean(Qtnlst.ChildNodes[3].ChildNodes[0].Attributes["marked"].Value);

                                                que.CorrectAnswer = "x=" + corx + " y=" + cory;
                                                uselect = uselect.Trim(',');
                                                que.UserSelection = uselect;
                                                QuestionDetail.Add(que);

                                            }
                                            else if (Qtnlst.LocalName == "dndQuestion")
                                            {
                                                QuestionDetails que = new QuestionDetails();
                                                Qtytype = Qtnlst.LocalName;
                                                que.QuestionType = Qtnlst.LocalName;
                                                que.AwardedPoints = Convert.ToInt32(Qtnlst.Attributes["awardedPoints"].Value);
                                                que.MaxPoints = Convert.ToInt32(Qtnlst.Attributes["maxPoints"].Value);
                                                que.QuizQuestion = Qtnlst.ChildNodes[0].InnerText;

                                                Dictionary<int, string> crtans = new Dictionary<int, string>();
                                                string uselect = "";
                                                string correctAns = "";
                                                foreach (XmlNode item in Qtnlst.ChildNodes[4].ChildNodes)
                                                {

                                                    int objinx = Convert.ToInt32(item.Attributes["objectIndex"].Value);
                                                    int desinx = Convert.ToInt32(item.Attributes["destinationIndex"].Value);
                                                    string objtxt = "", destxt = "";
                                                    if(objinx < 0 && desinx >= 0)
                                                    {
                                                        objtxt = "No Match";
                                                        destxt = Qtnlst.ChildNodes[3].ChildNodes[desinx].InnerText;
                                                    }
                                                   
                                                    else if (desinx < 0 && objinx >= 0)
                                                    {
                                                        objtxt = Qtnlst.ChildNodes[2].ChildNodes[objinx].InnerText;
                                                        destxt = "No Match";
                                                    }
                                                    else
                                                    {
                                                        objtxt = Qtnlst.ChildNodes[2].ChildNodes[objinx].InnerText;
                                                        destxt = Qtnlst.ChildNodes[3].ChildNodes[desinx].InnerText;
                                                    }

                                                    //string value = Convert.ToString(item.InnerText);
                                                    //crtans.Add(i, value);

                                                    correctAns += (objtxt + " - " + destxt) + ",";

                                                }
                                                foreach (XmlNode item in Qtnlst.ChildNodes[5].ChildNodes)
                                                {

                                                    int objinx = Convert.ToInt32(item.Attributes["objectIndex"].Value);
                                                    int desinx = Convert.ToInt32(item.Attributes["destinationIndex"].Value);

                                                     string objtxt = "", destxt = "";
                                                    if (objinx < 0 && desinx >= 0)
                                                    {                                                      
                                                        objtxt = "No Match";
                                                        destxt = Qtnlst.ChildNodes[3].ChildNodes[desinx].InnerText;
                                                    }
                                                    else if (desinx < 0 && objinx >= 0)
                                                    {
                                                        objtxt = Qtnlst.ChildNodes[2].ChildNodes[objinx].InnerText;
                                                        destxt = "No Match";
                                                    }
                                                    else 
                                                    {
                                                         objtxt = Qtnlst.ChildNodes[2].ChildNodes[objinx].InnerText;
                                                         destxt = Qtnlst.ChildNodes[3].ChildNodes[desinx].InnerText;

                                                    }                                                   

                                                    uselect += (objtxt + " - " + destxt) + ",";

                                                }
                                                correctAns = correctAns.Trim(',');
                                                uselect = uselect.Trim(',');
                                                que.CorrectAnswer = correctAns;
                                                que.UserSelection = uselect;

                                                QuestionDetail.Add(que);

                                            }
                                            else if (Qtnlst.LocalName == "likertScaleQuestion")// NEED TO DISCUSSTION , WHAT ANS TO STORED
                                            {
                                                QuestionDetails que = new QuestionDetails();
                                                Qtytype = Qtnlst.LocalName;
                                                que.QuestionType = Qtnlst.LocalName;
                                                que.AwardedPoints = 0;
                                                que.MaxPoints = 0;
                                                que.QuizQuestion = Qtnlst.ChildNodes[0].InnerText;

                                                string uselect = "";
                                                foreach (XmlNode item in Qtnlst.ChildNodes[3].ChildNodes)
                                                {

                                                    int stminx = Convert.ToInt32(item.Attributes["statementIndex"].Value);
                                                    int lblinx = Convert.ToInt32(item.Attributes["labelIndex"].Value);


                                                    string stmtxt = Qtnlst.ChildNodes[1].ChildNodes[stminx].InnerText;
                                                    string lbltxt = Qtnlst.ChildNodes[2].ChildNodes[lblinx].InnerText;

                                                    uselect += (stmtxt + " - " + lbltxt) + ",";
                                                }
                                                uselect = uselect.Trim(',');
                                                que.CorrectAnswer = "";
                                                que.UserSelection = uselect;

                                                QuestionDetail.Add(que);
                                            }
                                            else if (Qtnlst.LocalName == "essayQuestion")
                                            {
                                                QuestionDetails que = new QuestionDetails();
                                                Qtytype = Qtnlst.LocalName;
                                                que.QuestionType = Qtnlst.LocalName;
                                                que.QuizQuestion = Qtnlst.ChildNodes[0].InnerText;
                                                que.CorrectAnswer = "";
                                                que.AwardedPoints = 0;
                                                que.MaxPoints = 0;
                                                que.UserSelection = Qtnlst.ChildNodes[1].InnerText;

                                                QuestionDetail.Add(que);
                                            }
                                        }

                                    }

                                }

                            }

                            if (QuestionDetail != null)
                            {
                                if (QuestionDetail.Count > 0)
                                {
                                    string type = "";
                                    try
                                    {
                                        foreach (var item in QuestionDetail)
                                        {
                                            type = "";
                                            try
                                            {
                                                type = item.QuestionType;
                                                item.SummaryDetailId = quiz.Id;
                                                item.CreatedDate = DateTime.Now;
                                                _context.QuestionDetails.Add(item);
                                                _context.SaveChanges();

                                                var data = _context.XmlDetails.Where(x => x.SummaryDetailId == quiz.Id).FirstOrDefault();
                                                if (data != null)
                                                {
                                                    data.IsSuccess = true;
                                                    _context.XmlDetails.Update(data);
                                                    _context.SaveChanges();
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                throw ex;
                                            }
                                        }
                                        transaction.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        transaction.Rollback();

                                        string email = Request.Form["USER_EMAIL"];

                                        GenerateExcel(quiz.UserName == null ? "" : quiz.UserName, XmlContent);

                                        if (email != "" && email != null)
                                        {
                                            string bodyTemplate = System.IO.File.ReadAllText(Path.Combine("EmailTemplate/ErrorTemplate.html"));
                                            bodyTemplate = bodyTemplate.Replace("[UserName]", quiz.UserName == null ? "" : quiz.UserName);
                                            bodyTemplate = bodyTemplate.Replace("[Message]", ex.InnerException + " " + ex.Message);                                            
                                            bool objReturn = _emailSender.SendEmailAsyncWithBody(email, "Error Message", bodyTemplate, true);

                                        }

                                        WriteToFile(ex.InnerException + " " + ex.Message, quiz.UserName == null ? "" : quiz.UserName, type);

                                    }
                                }

                            }

                        }

                    }

                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    string email = Request.Form["USER_EMAIL"];

                    GenerateExcel(quiz.UserName == null ? "" : quiz.UserName, XmlContent);

                    if (email != "" && email != null)
                    {
                        string bodyTemplate = System.IO.File.ReadAllText(Path.Combine("EmailTemplate/ErrorTemplate.html"));
                        bodyTemplate = bodyTemplate.Replace("[UserName]", quiz.UserName == null ? "" : quiz.UserName);
                        bodyTemplate = bodyTemplate.Replace("[Message]", ex.InnerException + " " + ex.Message);
                        bool Issuccess = _emailSender.SendEmailAsyncWithBody(email, "Error Message", bodyTemplate, true);
                    }

                    WriteToFile(ex.InnerException + " " + ex.Message, quiz.UserName == null ? "" : quiz.UserName, Qtytype);
                    return Ok();
                }

            }
            //var result = new GenericResult<dynamic>();
            //result.Success = true;
            //return Ok(result);

            return Ok();
        }
        public void GenerateExcel(string UserName,string XmlContent)
        {  
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Excel";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Excel\\QuizExcel_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_');

            if (!System.IO.File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = System.IO.File.CreateText(filepath))
                {
                    
                   sw.WriteLine(XmlContent);
                    

                }
            }
            else
            {
                System.IO.File.Delete(filepath);

                using (StreamWriter sw = System.IO.File.CreateText(filepath))
                {                    
                  sw.WriteLine(XmlContent);
                  
                }
            }


        }

        public void WriteToFile(string Message, string UserName, string QuizType)
        {
            //C:\Project\Stuart Project\ISpringQuiz\Ispring_Quiz_Processor\Ispring_Quiz_Processor\Ispring_Quiz_Processor\bin\Debug\netcoreapp2.2\Logs\ServiceLog_4_1_2021.txt

            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\QuizLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";

            if (!System.IO.File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = System.IO.File.CreateText(filepath))
                {
                    if (UserName == "")
                    {
                        sw.WriteLine("Type Of Question: " + QuizType);
                        sw.WriteLine("Exception Message: " + Message);
                    }
                    else
                    {

                        sw.WriteLine("User Name: " + UserName + "   " + "Type Of Question: " + QuizType);
                        sw.WriteLine("Exception Message: " + Message);
                    }

                }
            }
            else
            {
                using (StreamWriter sw = System.IO.File.AppendText(filepath))
                {
                    if (UserName == "")
                    {
                        sw.WriteLine();
                        sw.WriteLine("Type Of Question: " + QuizType);
                        sw.WriteLine("Exception Message: " + Message);
                    }
                    else
                    {
                        sw.WriteLine();
                        sw.WriteLine("User Name: " + UserName + "   " + "Type Of Question: " + QuizType);
                        sw.WriteLine("Exception Message: " + Message);
                    }
                }
            }

        }

    }
}