using DAL.Common;
using DAL.Models;
using System.Security.Cryptography;
using System.Text;

namespace Sponge.Common
{
    public class CommonUtility
    {
        public void GenerateDynamicColumnNames(int? ConfigId, DateTime? PeriodFrom, DateTime? PeriodTO, string TimeLevel, string Frequency, int? TemplateId, out bool IsGroupColumnNameExist, string DocumentId = null)
        {
            IsGroupColumnNameExist = false;
           
            try
            {
                SPONGE_Context objContext = new SPONGE_Context();
                var GetColumnNames = objContext.SPG_CONFIG_STRUCTURE.Where(w => w.CONFIG_ID == ConfigId && w.COLLECTION_TYPE == "Measure" && w.GROUPCOLUMNNAME != null).Select(s => new { DisplayName = s.DISPLAY_NAME, DataType = s.DATA_TYPE, ConfigUserId = s.CONFIGUSER_ID }).Distinct().OrderBy(o => o.ConfigUserId).ToList();

                var start = new DateTime(PeriodFrom.Value.Year, PeriodFrom.Value.Month, PeriodFrom.Value.Day);
                var end = new DateTime(PeriodTO.Value.Year, PeriodTO.Value.Month, PeriodTO.Value.Day);
                string GetFinancialYear = Convert.ToString(PeriodFrom.Value.Year) + "-" + Convert.ToString(Convert.ToInt64(PeriodFrom.Value.Year + 1)) + "-";
                if (TimeLevel.ToUpper() == "MONTHLY" && Frequency.ToUpper() == "YEARLY" && GetColumnNames.Count > 0)
                {
                    // set end-date to end of month
                    end = new DateTime(end.Year, end.Month, DateTime.DaysInMonth(end.Year, end.Month));
                    var GetMonthNames = Enumerable.Range(0, Int32.MaxValue)
                                        .Select(e => start.AddMonths(e))
                                        .TakeWhile(e => e <= end)
                                        .Select(e => e.ToString("MMM").Trim() + "-" + e.Year.ToString().Trim()).ToList();

                    foreach (var ColNames in GetColumnNames.Take(12))
                    {
                        foreach (var MonthNames in GetMonthNames)
                        {
                            if (MonthNames.Substring(0, 3).ToUpper() == ColNames.DisplayName.ToUpper())
                            {
                                string MonthVal = ColNames.DisplayName;
                                var GetMonth = (Int16)((Helper.MMM)Enum.Parse(typeof(Helper.MMM), MonthVal));
                                string GetYear = MonthNames.Substring(4, MonthNames.LastIndexOf("-") + 1).Trim();
                                //  int GetMonth = DateTime.ParseExact(ColNames.DisplayName, "MMM", CultureInfo.CurrentCulture).Month;
                                SPG_GETTIMECODE objtimeCode = new SPG_GETTIMECODE();
                                objtimeCode.CONFIG_ID = ConfigId;
                                objtimeCode.DATA_TYPE = ColNames.DataType;
                                objtimeCode.DISPLAY_NAME = MonthNames;
                                objtimeCode.DOCUMENT_ID = DocumentId;
                                objtimeCode.TEMPLATE_ID = TemplateId;
                                objtimeCode.FORTIMECODE = GetYear + "" + string.Format("{00:00}", GetMonth).Trim().TrimEnd().TrimStart();
                                objtimeCode.COLUMN_CODE = ColNames.DisplayName + "-" + ColNames.DataType;
                                objContext.SPG_GETTIMECODE.Add(objtimeCode);
                                objContext.SaveChanges();
                                var TimeCodeId = objtimeCode.CONFIG_TIMECODE_ID;
                                break;
                            }
                        }
                    }
                    IsGroupColumnNameExist = true;

                }
                else if (Frequency.ToUpper() == "YEARLY" && TimeLevel.ToUpper() == "HALF_YEARLY" && GetColumnNames.Count > 0)
                {


                    var GetYearlyAndHalfYearly = GetColumnNames.Select(s => new { DisplayName = GetFinancialYear.Trim() + "" + s.DisplayName.Trim(), DataType = s.DataType }).ToList();

                    foreach (var item in GetYearlyAndHalfYearly)
                    {

                        SPG_GETTIMECODE objtimeCode = new SPG_GETTIMECODE();
                        objtimeCode.CONFIG_ID = ConfigId;
                        objtimeCode.DATA_TYPE = item.DataType;
                        objtimeCode.DISPLAY_NAME = item.DisplayName;
                        objtimeCode.DOCUMENT_ID = DocumentId;
                        objtimeCode.TEMPLATE_ID = TemplateId;
                        if (item.DisplayName.Contains("H1"))
                        {
                            objtimeCode.FORTIMECODE = PeriodFrom.Value.Year + "07" + PeriodFrom.Value.Year + "12".Trim();
                        }
                        if (item.DisplayName.Contains("H2"))
                        {
                            objtimeCode.FORTIMECODE = PeriodTO.Value.Year + "01" + PeriodTO.Value.Year + "06".Trim();
                        }
                        objContext.SPG_GETTIMECODE.Add(objtimeCode);
                        objContext.SaveChanges();
                        var TimeCodeId = objtimeCode.CONFIG_TIMECODE_ID;
                    }
                    IsGroupColumnNameExist = true;
                }
                else if (Frequency.ToUpper() == "YEARLY" && TimeLevel.ToUpper() == "QUARTERLY" && GetColumnNames.Count > 0)
                {
                    var GetYearlyAndQuartely = GetColumnNames.Select(s => new { DisplayName = GetFinancialYear.Trim() + "" + s.DisplayName.Trim(), DataType = s.DataType }).ToList();
                    foreach (var item in GetYearlyAndQuartely)
                    {

                        SPG_GETTIMECODE objtimeCode = new SPG_GETTIMECODE();
                        objtimeCode.CONFIG_ID = ConfigId;
                        objtimeCode.DATA_TYPE = item.DataType;
                        objtimeCode.DISPLAY_NAME = item.DisplayName;
                        objtimeCode.DOCUMENT_ID = DocumentId;
                        objtimeCode.TEMPLATE_ID = TemplateId;
                        if (item.DisplayName.Contains("Q1"))
                        {
                            objtimeCode.FORTIMECODE = PeriodFrom.Value.Year + "07" + PeriodFrom.Value.Year + "09".Trim();
                        }
                        else if (item.DisplayName.Contains("Q2"))
                        {
                            objtimeCode.FORTIMECODE = PeriodFrom.Value.Year + "10" + PeriodFrom.Value.Year + "12".Trim();
                        }
                        else if (item.DisplayName.Contains("Q3"))
                        {
                            objtimeCode.FORTIMECODE = PeriodTO.Value.Year + "01" + PeriodTO.Value.Year + "03".Trim();
                        }
                        else if (item.DisplayName.Contains("Q4"))
                        {
                            objtimeCode.FORTIMECODE = PeriodTO.Value.Year + "04" + PeriodTO.Value.Year + "06".Trim();
                        }
                        objContext.SPG_GETTIMECODE.Add(objtimeCode);
                        objContext.SaveChanges();
                        var TimeCodeId = objtimeCode.CONFIG_TIMECODE_ID;
                    }
                    IsGroupColumnNameExist = true;
                }
                else if (Frequency.ToUpper() == "HALF_YEARLY" && TimeLevel.ToUpper() == "QUARTERLY" && GetColumnNames.Count > 0)
                {
                    //var GetHalfYearlyAndQuartly = GetColumnNames.Select(s => new { DisplayName = GetFinancialYear.Trim() + "" + s.DisplayName.Trim(), DataType = s.DataType }).ToList();
                    var GetHalfYearlyAndQuartly = Enumerable.Range(0, Int32.MaxValue)
                                      .Select(e => start.AddMonths(e))
                                      .TakeWhile(e => e <= end)
                                      .Select(e => e.ToString("MMM").Trim() + "-" + e.Year.ToString().Trim()).ToList();
                    int i = 0;
                    foreach (var colname in GetColumnNames)
                    {

                        foreach (var item in GetHalfYearlyAndQuartly)
                        {
                            string MonthVal = GetHalfYearlyAndQuartly[i].Substring(0, 3);
                            var GetMonth = (Int16)((Helper.MMM)Enum.Parse(typeof(Helper.MMM), MonthVal));
                            string GetYear = GetHalfYearlyAndQuartly[i].Substring(4, GetHalfYearlyAndQuartly[i].LastIndexOf("-") + 1).Trim();
                            SPG_GETTIMECODE objtimeCode = new SPG_GETTIMECODE();
                            objtimeCode.CONFIG_ID = ConfigId;
                            objtimeCode.DOCUMENT_ID = DocumentId;
                            objtimeCode.TEMPLATE_ID = TemplateId;
                            objtimeCode.DATA_TYPE = colname.DisplayName;
                            if (i <= 5)
                            {
                                if (i > 0 && i < 3)
                                    objtimeCode.DISPLAY_NAME = "H1-Q1";
                                else if (i <= 5)
                                    objtimeCode.DISPLAY_NAME = "H1-Q2";
                                objtimeCode.FORTIMECODE = GetYear + "" + string.Format("{00:00}", GetMonth).Trim().TrimEnd().TrimStart();

                            }
                            else if (i > 5)
                            {
                                if (i >= 6 && i < 9)
                                    objtimeCode.DISPLAY_NAME = "H2-Q1";
                                else if (i >= 9)
                                    objtimeCode.DISPLAY_NAME = "H2-Q2";
                                objtimeCode.FORTIMECODE = GetYear + "" + string.Format("{00:00}", GetMonth).Trim().TrimEnd().TrimStart();
                                i++;
                            }
                            objContext.SPG_GETTIMECODE.Add(objtimeCode);
                            objContext.SaveChanges();
                            var TimeCodeId = objtimeCode.CONFIG_TIMECODE_ID;

                            if (i == 5)
                            {
                                i++;
                                break;
                            }
                        }


                    }
                    IsGroupColumnNameExist = true;
                }
                else if (Frequency.ToUpper() == "HALF_YEARLY" && TimeLevel.ToUpper() == "MONTHLY" && GetColumnNames.Count > 0)
                {

                    var GetHalfyearlyAndMonthly = Enumerable.Range(0, Int32.MaxValue)
                                        .Select(e => start.AddMonths(e))
                                        .TakeWhile(e => e <= end)
                                        .Select(e => e.ToString("MMM").Trim() + "-" + e.Year.ToString().Trim()).ToList();
                    if (GetHalfyearlyAndMonthly.Count > 6)
                    {
                        GetHalfyearlyAndMonthly = GetHalfyearlyAndMonthly.Take(6).ToList();
                    }
                    int i = 0;
                    foreach (var item in GetColumnNames.Take(6))
                    {

                        string MonthVal = GetHalfyearlyAndMonthly[i].Substring(0, 3);
                        var GetMonth = (Int16)((Helper.MMM)Enum.Parse(typeof(Helper.MMM), MonthVal));
                        string GetYear = GetHalfyearlyAndMonthly[i].Substring(4, GetHalfyearlyAndMonthly[i].LastIndexOf("-") + 1).Trim();
                        SPG_GETTIMECODE objtimeCode = new SPG_GETTIMECODE();
                        objtimeCode.CONFIG_ID = ConfigId;
                        objtimeCode.DATA_TYPE = item.DataType;
                        objtimeCode.DISPLAY_NAME = GetHalfyearlyAndMonthly[i];
                        objtimeCode.DOCUMENT_ID = DocumentId;
                        objtimeCode.TEMPLATE_ID = TemplateId;
                        objtimeCode.FORTIMECODE = GetYear + "" + string.Format("{00:00}", GetMonth).Trim().TrimEnd().TrimStart();
                        objContext.SPG_GETTIMECODE.Add(objtimeCode);
                        objContext.SaveChanges();
                        var TimeCodeId = objtimeCode.CONFIG_TIMECODE_ID;
                        i++;
                    }
                    IsGroupColumnNameExist = true;

                }
                else if (Frequency.ToUpper() == "QUARTERLY" && TimeLevel.ToUpper() == "MONTHLY" && GetColumnNames.Count > 0)
                {

                    var GetQuartelyAndMonthly = Enumerable.Range(0, Int32.MaxValue)
                                       .Select(e => start.AddMonths(e))
                                       .TakeWhile(e => e <= end)
                                       .Select(e => e.ToString("MMM").Trim() + "-" + e.Year.ToString().Trim()).ToList();
                    if (GetQuartelyAndMonthly.Count > 3)
                    {
                        GetQuartelyAndMonthly = GetQuartelyAndMonthly.Take(3).ToList();
                    }
                    int i = 0;
                    foreach (var item in GetColumnNames.Take(3))
                    {

                        string MonthVal = GetQuartelyAndMonthly[i].Substring(0, 3);
                        var GetMonth = (Int16)((Helper.MMM)Enum.Parse(typeof(Helper.MMM), MonthVal));
                        string GetYear = GetQuartelyAndMonthly[i].Substring(4, GetQuartelyAndMonthly[i].LastIndexOf("-") + 1).Trim();
                        SPG_GETTIMECODE objtimeCode = new SPG_GETTIMECODE();
                        objtimeCode.CONFIG_ID = ConfigId;
                        objtimeCode.DATA_TYPE = item.DataType;
                        objtimeCode.DISPLAY_NAME = GetQuartelyAndMonthly[i];
                        objtimeCode.DOCUMENT_ID = DocumentId;
                        objtimeCode.TEMPLATE_ID = TemplateId;
                        objtimeCode.FORTIMECODE = GetYear + "" + string.Format("{00:00}", GetMonth).Trim().TrimEnd().TrimStart();
                        objContext.SPG_GETTIMECODE.Add(objtimeCode);
                        objContext.SaveChanges();
                        var TimeCodeId = objtimeCode.CONFIG_TIMECODE_ID;
                        i++;
                    }
                    IsGroupColumnNameExist = true;
                }
                else if (Frequency.ToUpper() == "QUARTERLY" && TimeLevel.ToUpper() == "WEEKLY" && GetColumnNames.Count > 0)
                {

                    var GetQuartelyAndWeekly = Enumerable.Range(0, Int32.MaxValue)
                                      .Select(e => start.AddMonths(e))
                                      .TakeWhile(e => e <= end)
                                      .Select(e => e.ToString("MMM").Trim() + "-".ToString().Trim()).ToList();
                    if (GetQuartelyAndWeekly.Count > 3)
                    {
                        GetQuartelyAndWeekly = GetQuartelyAndWeekly.Take(3).ToList();
                    }
                    int i = 0;
                    foreach (var item in GetColumnNames.Take(12))
                    {
                        string MonthVal = GetQuartelyAndWeekly[i].Substring(0, 3);
                        var GetMonth = (Int16)((Helper.MMM)Enum.Parse(typeof(Helper.MMM), MonthVal));
                        string GetYear = GetQuartelyAndWeekly[i].Substring(4, GetQuartelyAndWeekly[i].LastIndexOf("-") + 1).Trim();
                        SPG_GETTIMECODE objtimeCode = new SPG_GETTIMECODE();
                        objtimeCode.CONFIG_ID = ConfigId;
                        objtimeCode.DATA_TYPE = item.DataType;
                        objtimeCode.DOCUMENT_ID = DocumentId;
                        objtimeCode.TEMPLATE_ID = TemplateId;
                        if (item.DisplayName.Contains("W1") || item.DisplayName.Contains("W2") || item.DisplayName.Contains("W3") || item.DisplayName.Contains("W4"))
                        {
                            MonthVal = GetQuartelyAndWeekly[0].Substring(0, 3);
                            GetMonth = (Int16)((Helper.MMM)Enum.Parse(typeof(Helper.MMM), MonthVal));
                            GetYear = GetQuartelyAndWeekly[0].Substring(4, GetQuartelyAndWeekly[0].LastIndexOf("-") + 1).Trim();
                            objtimeCode.DISPLAY_NAME = GetQuartelyAndWeekly[0] + "" + item.DisplayName;
                            objtimeCode.FORTIMECODE = GetYear + "" + string.Format("{00:00}", GetMonth).Trim().TrimEnd().TrimStart();
                        }
                        if (item.DisplayName.Contains("W5") || item.DisplayName.Contains("W6") || item.DisplayName.Contains("W7") || item.DisplayName.Contains("W8"))
                        {
                            MonthVal = GetQuartelyAndWeekly[1].Substring(0, 3);
                            GetMonth = (Int16)((Helper.MMM)Enum.Parse(typeof(Helper.MMM), MonthVal));
                            GetYear = GetQuartelyAndWeekly[1].Substring(4, GetQuartelyAndWeekly[1].LastIndexOf("-") + 1).Trim();
                            objtimeCode.DISPLAY_NAME = GetQuartelyAndWeekly[1] + "" + item.DisplayName;
                            objtimeCode.FORTIMECODE = GetYear + "" + string.Format("{00:00}", GetMonth).Trim().TrimEnd().TrimStart();
                        }
                        if (item.DisplayName.Contains("W9") || item.DisplayName.Contains("W10") || item.DisplayName.Contains("W11") || item.DisplayName.Contains("W12"))
                        {
                            MonthVal = GetQuartelyAndWeekly[2].Substring(0, 3);
                            GetMonth = (Int16)((Helper.MMM)Enum.Parse(typeof(Helper.MMM), MonthVal));
                            GetYear = GetQuartelyAndWeekly[2].Substring(4, GetQuartelyAndWeekly[2].LastIndexOf("-") + 1).Trim();
                            objtimeCode.DISPLAY_NAME = GetQuartelyAndWeekly[2] + "" + item.DisplayName;
                            objtimeCode.FORTIMECODE = GetYear + "" + string.Format("{00:00}", GetMonth).Trim().TrimEnd().TrimStart();
                        }

                        objContext.SPG_GETTIMECODE.Add(objtimeCode);
                        objContext.SaveChanges();
                        var TimeCodeId = objtimeCode.CONFIG_TIMECODE_ID;
                        i++;
                    }
                    IsGroupColumnNameExist = true;

                }
                else if (Frequency.ToUpper() == "MONTHLY" && TimeLevel.ToUpper() == "WEEKLY" && GetColumnNames.Count > 0)
                {
                    var GetMonthlyAndWeekly = Enumerable.Range(0, Int32.MaxValue)
                                     .Select(e => start.AddMonths(e))
                                     .TakeWhile(e => e <= end)
                                     .Select(e => e.ToString("MMM").Trim() + "-" + e.Year.ToString().Trim()).ToList();
                    int i = 0;
                    foreach (var item in GetColumnNames.Take(4))
                    {
                        string MonthVal = "";
                        var GetMonth = 0;
                        string GetYear = "";
                        SPG_GETTIMECODE objtimeCode = new SPG_GETTIMECODE();
                        objtimeCode.CONFIG_ID = ConfigId;
                        objtimeCode.DATA_TYPE = item.DataType;
                        objtimeCode.DOCUMENT_ID = DocumentId;
                        objtimeCode.TEMPLATE_ID = TemplateId;
                        if (item.DisplayName.Contains("W1"))
                        {
                            MonthVal = GetMonthlyAndWeekly[0].Substring(0, 3);//Retrun Month Name eg. July-
                            GetMonth = (Int16)((Helper.MMM)Enum.Parse(typeof(Helper.MMM), MonthVal));//Retrun Month value eg. 07
                            GetYear = GetMonthlyAndWeekly[0].Substring(4, GetMonthlyAndWeekly[0].LastIndexOf("-") + 1).Trim();
                            objtimeCode.DISPLAY_NAME = GetMonthlyAndWeekly[0].Trim() + "" + item.DisplayName.Trim();//Return column name e.g July-W1
                            objtimeCode.FORTIMECODE = GetYear + "" + string.Format("{00:00}", GetMonth).Trim().TrimEnd().TrimStart();//Retuen ForTimeCode e.g 201807
                        }
                        else if (item.DisplayName.Contains("W2"))
                        {
                            MonthVal = GetMonthlyAndWeekly[0].Substring(0, 3);
                            GetMonth = (Int16)((Helper.MMM)Enum.Parse(typeof(Helper.MMM), MonthVal));
                            GetYear = GetMonthlyAndWeekly[0].Substring(4, GetMonthlyAndWeekly[0].LastIndexOf("-") + 1).Trim();
                            objtimeCode.DISPLAY_NAME = GetMonthlyAndWeekly[0].Trim() + "" + item.DisplayName.Trim();
                            objtimeCode.FORTIMECODE = GetYear + "" + string.Format("{00:00}", GetMonth).Trim().TrimEnd().TrimStart();
                        }
                        else if (item.DisplayName.Contains("W3"))
                        {
                            MonthVal = GetMonthlyAndWeekly[0].Substring(0, 3);
                            GetMonth = (Int16)((Helper.MMM)Enum.Parse(typeof(Helper.MMM), MonthVal));
                            GetYear = GetMonthlyAndWeekly[0].Substring(4, GetMonthlyAndWeekly[0].LastIndexOf("-") + 1).Trim();
                            objtimeCode.DISPLAY_NAME = GetMonthlyAndWeekly[0].Trim() + "" + item.DisplayName.Trim();
                            objtimeCode.FORTIMECODE = GetYear + "" + string.Format("{00:00}", GetMonth).Trim().TrimEnd().TrimStart();
                        }
                        else if (item.DisplayName.Contains("W4"))
                        {
                            MonthVal = GetMonthlyAndWeekly[0].Substring(0, 3);
                            GetMonth = (Int16)((Helper.MMM)Enum.Parse(typeof(Helper.MMM), MonthVal));
                            GetYear = GetMonthlyAndWeekly[0].Substring(4, GetMonthlyAndWeekly[0].LastIndexOf("-") + 1).Trim();
                            objtimeCode.DISPLAY_NAME = GetMonthlyAndWeekly[0].Trim() + "" + item.DisplayName.Trim();
                            objtimeCode.FORTIMECODE = GetYear + "" + string.Format("{00:00}", GetMonth).Trim().TrimEnd().TrimStart();
                        }
                        objContext.SPG_GETTIMECODE.Add(objtimeCode);
                        objContext.SaveChanges();
                        var TimeCodeId = objtimeCode.CONFIG_TIMECODE_ID;
                        i++;
                    }
                    IsGroupColumnNameExist = true;
                }
            }
            catch (Exception ex)
            {
                ErrorLog lgr = new ErrorLog();
                lgr.LogErrorInTextFile(ex);
                SentErrorMail.SentEmailtoError("Error on Batch Job  while  Generation Dynamic Header \n \n InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
            }


        }
    }


    public static class Helper
    {

        public enum Month
        {
            January = 1,
            February,
            March,
            April,
            May,
            June,
            July,
            August,
            September,
            October,
            November,
            December
        }
        public enum MMM : Int16
        {
            Jan = 01,
            Feb = 02,
            Mar = 03,
            Apr = 04,
            May = 05,
            Jun = 06,
            Jul = 07,
            Aug = 08,
            Sep = 09,
            Oct = 10,
            Nov = 11,
            Dec = 12
        }
        public static class Constant
        {
            public const string Date = "DATE";
            public const string YEARLY = "YEARLY";
            public const string HALFYEARLY = "HALF_YEARLY";
            public const string QUARTERLY = "QUARTERLY";
            public const string WEEKLY = "WEEKLY";
            public const string MONTHLY = "MONTHLY";
            public const string DAILY = "DAILY";

        }

        public static class ReportingPeriod
        {
            public const string CURRENT = "CURRENT";
            public const string PREVIOUS = "PREVIOUS";
            public const string NEXT = "NEXT";
        }

        public static class ApprovalStatus
        {
            public const string Pending = "Pending";
            public const string Rejected = "Rejected";
            public const string Abandon = "Abandon";
            public const string Approved = "Approved";


        }

        public enum ApprovalStatusEnum
        {
            Pending = 1,
            Rejected = 3,
            Abandon = 4,
            Approved = 2,
            Draft = 0
        }
        public static String encodeWithKey(String textIn)
        {
            //String key = ConfigurationManager.AppSettings["Key"].ToString();
            String key = "";
            var _crypt = new TripleDESCryptoServiceProvider();
            var _hashmd5 = new MD5CryptoServiceProvider();

            var _byteHash = _hashmd5.ComputeHash(Encoding.UTF8.GetBytes(key));
            var _byteText = Encoding.UTF8.GetBytes(textIn);

            _crypt.Key = _byteHash;
            _crypt.Mode = CipherMode.ECB;

            var _encodeByte = _crypt.CreateEncryptor().TransformFinalBlock(_byteText, 0, _byteText.Length);

            return Convert.ToBase64String(_encodeByte);
        }
        public static String decodeWithKey(String encode)
        {
            //String key = ConfigurationManager.AppSettings["Key"].ToString();
            String key = "";
            var _crypt = new TripleDESCryptoServiceProvider();
            var _hashmd5 = new MD5CryptoServiceProvider();

            var _byteHash = _hashmd5.ComputeHash(Encoding.UTF8.GetBytes(key));
            var _byteText = Convert.FromBase64String(encode);

            _crypt.Key = _byteHash;
            _crypt.Mode = CipherMode.ECB;

            var decodeByte = _crypt.CreateDecryptor().TransformFinalBlock(_byteText, 0, _byteText.Length);

            return Encoding.UTF8.GetString(decodeByte);
        }
        public static string encrypt(string encryptString)
        {
            string EncryptionKey = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            byte[] clearBytes = Encoding.Unicode.GetBytes(encryptString);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    encryptString = Convert.ToBase64String(ms.ToArray());
                }
            }
            return encryptString.Replace("/", "portaldev");
        }

        public static string Decrypt(string cipherText)
        {
            string EncryptionKey = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            cipherText = cipherText.Replace(" ", "+").Replace("portaldev", "/");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

    }
}