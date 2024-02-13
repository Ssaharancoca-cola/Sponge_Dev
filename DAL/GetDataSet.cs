using System.Data;
using System.Reflection;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using DAL.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;

namespace DAL
{
    public class GetDataSet : IDisposable
    {
        void IDisposable.Dispose()
        {

        }
        //public DataSet GetDataSetValueTest(string selectCommand)
        //{
        //    var result = new DataSet();
        //    using (var context = new SPONGE_Context())
        //    {
        //        using (var transaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted))
        //        {
        //            using (var cmd = context.Database.Connection.CreateCommand())
        //            {
        //                cmd.Transaction = transaction.UnderlyingTransaction;
        //                //var cmd = context.Database.Connection.CreateCommand();
        //                cmd.CommandText = selectCommand;
        //                cmd.CommandType = CommandType.StoredProcedure;

        //                OracleParameter parameter2 = new OracleParameter();
        //                parameter2.ParameterName = "Config_C";
        //                parameter2.Direction = ParameterDirection.Output;
        //                parameter2.OracleDbType = OracleDbType.RefCursor;

        //                cmd.Parameters.Add(parameter2);

        //                try
        //                {
        //                    // context.Database.Connection.Open();
        //                    var dtDataTable = new DataTable();
        //                    var reader = cmd.ExecuteReader();
        //                    do
        //                    {
        //                        dtDataTable.Load(reader);
        //                        result.Tables.Add(dtDataTable);

        //                    } while (!reader.IsClosed);
        //                }
        //                catch (Exception ex)
        //                {
        //                    ErrorLog lgerr = new ErrorLog();
        //                    lgerr.LogErrorInTextFile(ex);
        //                    SentErrorMail.SentEmailtoError("InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //                }
        //                finally
        //                {
        //                    context.Database.Connection.Close();
        //                }
        //            }
        //            return result;
        //        }
        //    }
        //}

        //static OracleParameter ToConvertSqlParams(OracleCommand command, string name, object value)
        //{
        //    var p = command.CreateParameter();
        //    p.ParameterName = name;
        //    p.Value = value;
        //    return p;
        //}
        public DataSet GetBatchJobDataSetValue(string selectCommand, int configid, string p_IS_PREPOPULATE)
        {
            var result = new DataSet();
            using (var context = new SPONGE_Context())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    using (var cmd = context.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.Transaction = transaction.GetDbTransaction();
                        cmd.CommandText = selectCommand;
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter("@p_config_id", configid));
                        cmd.Parameters.Add(new SqlParameter("@p_IS_PREPOPULATE", p_IS_PREPOPULATE));

                        try
                        {
                            var dtDataTable = new DataTable();
                            var reader = cmd.ExecuteReader();
                            do
                            {
                                dtDataTable.Load(reader);
                                result.Tables.Add(dtDataTable);

                            } while (!reader.IsClosed);
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            ErrorLog lgerr = new ErrorLog();
                            lgerr.LogErrorInTextFile(ex);
                            SentErrorMail.SentEmailtoError("InnerException: "
                            + ex.InnerException.ToString()
                            + " StackTrace: " + ex.StackTrace.ToString()
                            + " Message" + ex.Message);
                            transaction.Rollback();
                        }
                        finally
                        {
                            context.Database.CloseConnection();
                        }
                    }
                }
            }
            return result;
        }
        //public DataSet GetDataSetValueDTField(string selectCommand, decimal? subjectAreaId, string user_id, string DataType)
        //{
        //    var result = new DataSet();
        //    using (var context = new SPONGE_Context())
        //    {
        //        using (var transaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted))
        //        {
        //            using (var cmd = context.Database.Connection.CreateCommand())
        //            {
        //                cmd.Transaction = transaction.UnderlyingTransaction;
        //                //var cmd = context.Database.Connection.CreateCommand();
        //                cmd.CommandText = selectCommand;
        //                cmd.CommandType = CommandType.StoredProcedure;
        //                OracleParameter param2 = new OracleParameter();
        //                param2.ParameterName = "p_userID";
        //                param2.OracleDbType = OracleDbType.Varchar2;
        //                param2.Direction = ParameterDirection.Input;
        //                param2.Value = user_id;
        //                OracleParameter param = new OracleParameter();
        //                param.ParameterName = "p_subjectAreaID";
        //                param.OracleDbType = OracleDbType.Int16;
        //                param.Direction = ParameterDirection.Input;
        //                param.Value = Convert.ToInt16(subjectAreaId);
        //                ;
        //                OracleParameter param3 = new OracleParameter();
        //                param3.ParameterName = "p_DATA_TYPE";
        //                param3.OracleDbType = OracleDbType.Varchar2;
        //                param3.Direction = ParameterDirection.Input;
        //                param3.Value = DataType;
        //                OracleParameter parameter2 = new OracleParameter();
        //                parameter2.ParameterName = "Config_C";
        //                parameter2.Direction = ParameterDirection.Output;
        //                parameter2.OracleDbType = OracleDbType.RefCursor;
        //                cmd.Parameters.Add(param);
        //                cmd.Parameters.Add(param2);

        //                cmd.Parameters.Add(param3);
        //                cmd.Parameters.Add(parameter2);

        //                try
        //                {
        //                    // context.Database.Connection.Open();
        //                    var dtDataTable = new DataTable();
        //                    var reader = cmd.ExecuteReader();
        //                    do
        //                    {
        //                        dtDataTable.Load(reader);
        //                        result.Tables.Add(dtDataTable);

        //                    } while (!reader.IsClosed);
        //                }
        //                catch (Exception ex)
        //                {
        //                    ErrorLog lgerr = new ErrorLog();
        //                    lgerr.LogErrorInTextFile(ex);
        //                    SentErrorMail.SentEmailtoError("InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //                }
        //                finally
        //                {
        //                    context.Database.Connection.Close();
        //                }
        //            }
        //            return result;

        //        }
        //    }
        //}
        //public DataSet ExecuteSP(string selectCommand, string UserId)
        //{
        //    var result = new DataSet();
        //    using (var context = new SPONGE_Context())
        //    {
        //        using (var transaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted))
        //        {
        //            using (var cmd = context.Database.Connection.CreateCommand())
        //            {
        //                cmd.Transaction = transaction.UnderlyingTransaction;
        //                //var cmd = context.Database.Connection.CreateCommand();
        //                cmd.CommandText = selectCommand;
        //                cmd.CommandType = CommandType.StoredProcedure;

        //                cmd.Parameters.Add(CreateOracleParameter(UserId, "p_USERID"));
        //                cmd.Parameters.Add(CreateOracleParameter("Config_C"));

        //                try
        //                {
        //                    //context.Database.Connection.Open();
        //                    var dtDataTable = new DataTable();
        //                    var reader = cmd.ExecuteReader();
        //                    do
        //                    {
        //                        dtDataTable.Load(reader);
        //                        result.Tables.Add(dtDataTable);

        //                    } while (!reader.IsClosed);
        //                }
        //                catch (Exception ex)
        //                {
        //                    ErrorLog lgerr = new ErrorLog();
        //                    lgerr.LogErrorInTextFile(ex);
        //                    SentErrorMail.SentEmailtoError("InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //                }
        //                finally
        //                {
        //                    context.Database.Connection.Close();
        //                }
        //            }
        //            return result;
        //        }
        //    }
        //}
        public DataSet GetDataSetValue(string selectCommand, int configid)
        {
            var result = new DataSet();
            using (var context = new SPONGE_Context())
            {
                using (var command = context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = selectCommand;
                    command.CommandType = CommandType.StoredProcedure;

                    var param = new SqlParameter("@P_configID", SqlDbType.Int);
                    param.Value = configid;
                    command.Parameters.Add(param);

                    try
                    {
                        context.Database.OpenConnection();
                        using (var reader = command.ExecuteReader())
                        {
                            do
                            {
                                var dtDataTable = new DataTable();
                                dtDataTable.Load(reader);
                                result.Tables.Add(dtDataTable);

                            } while (!reader.IsClosed);
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLog lgerr = new ErrorLog();
                        lgerr.LogErrorInTextFile(ex);
                        SentErrorMail.SentEmailtoError("InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
                    }
                    finally
                    {
                        context.Database.CloseConnection();
                    }
                }
            }
            return result;
        }
        //public DataSet GetOnlinetemplateDataSetValue(string selectCommand, int configid, string effectiveDate)
        //{
        //    var result = new DataSet();
        //    using (var context = new SPONGE_Context())
        //    {
        //        using (var transaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted))
        //        {
        //            using (var cmd = context.Database.Connection.CreateCommand())
        //            {
        //                cmd.Transaction = transaction.UnderlyingTransaction;
        //                //var cmd = context.Database.Connection.CreateCommand();
        //                cmd.CommandText = selectCommand;
        //                cmd.CommandType = CommandType.StoredProcedure;

        //                cmd.Parameters.Add(CreateOracleParameter(configid, "p_ConfigID"));
        //                cmd.Parameters.Add(CreateOracleParameter(effectiveDate, "p_EffectiveToDate"));
        //                cmd.Parameters.Add(CreateOracleParameter("Config_C"));

        //                try
        //                {
        //                    // context.Database.Connection.Open();
        //                    var dtDataTable = new DataTable();
        //                    var reader = cmd.ExecuteReader();
        //                    do
        //                    {
        //                        dtDataTable.Load(reader);
        //                        result.Tables.Add(dtDataTable);

        //                    } while (!reader.IsClosed);
        //                }
        //                catch (Exception ex)
        //                {
        //                    ErrorLog lgerr = new ErrorLog();
        //                    lgerr.LogErrorInTextFile(ex);
        //                    SentErrorMail.SentEmailtoError("InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //                }
        //                finally
        //                {
        //                    context.Database.Connection.Close();
        //                }
        //            }
        //            return result;
        //        }
        //    }
        //}
        //public DataSet GetOnlinetemplateDataDropdown(string selectCommand, int configid, string effectiveDate)
        //{
        //    var result = new DataSet();
        //    using (var context = new SPONGE_Context())
        //    {
        //        using (var transaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted))
        //        {
        //            using (var cmd = context.Database.Connection.CreateCommand())
        //            {
        //                cmd.Transaction = transaction.UnderlyingTransaction;
        //                //var cmd = context.Database.Connection.CreateCommand();
        //                cmd.CommandText = selectCommand;
        //                cmd.CommandType = CommandType.StoredProcedure;

        //                cmd.Parameters.Add(CreateOracleParameter(configid, "p_ConfigID"));
        //                cmd.Parameters.Add(CreateOracleParameter(effectiveDate, "p_EffectiveToDate"));
        //                cmd.Parameters.Add(CreateOracleParameter("Config_C"));

        //                try
        //                {
        //                    // context.Database.Connection.Open();
        //                    var dtDataTable = new DataTable();
        //                    var reader = cmd.ExecuteReader();
        //                    do
        //                    {
        //                        dtDataTable.Load(reader);
        //                        result.Tables.Add(dtDataTable);

        //                    } while (!reader.IsClosed);
        //                }
        //                catch (Exception ex)
        //                {
        //                    ErrorLog lgerr = new ErrorLog();
        //                    lgerr.LogErrorInTextFile(ex);
        //                    SentErrorMail.SentEmailtoError("InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //                }
        //                finally
        //                {
        //                    context.Database.Connection.Close();
        //                }
        //            }
        //            return result;
        //        }
        //    }
        //}
        //public void GetHiddenColumn(string selectCommand, int configid, string columnvalue, out string result, out string DropdownColumn)
        //{
        //    result = "";
        //    DropdownColumn = "";
        //    using (var context = new SPONGE_Context())
        //    {
        //        using (var transaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted))
        //        {
        //            using (var cmd = context.Database.Connection.CreateCommand())
        //            {
        //                cmd.Transaction = transaction.UnderlyingTransaction;
        //                //var cmd = context.Database.Connection.CreateCommand();
        //                cmd.CommandText = selectCommand;
        //                cmd.CommandType = CommandType.StoredProcedure;
        //                cmd.Parameters.Add(CreateOracleParameter(columnvalue, "p_MasterColumn"));
        //                cmd.Parameters.Add(CreateOracleParameter(configid, "p_configid"));
        //                cmd.Parameters.Add(CreateOracleParameter("Hiddencolumn", ParameterDirection.Output));
        //                cmd.Parameters.Add(CreateOracleParameter("DROPDOWNCOLUMN", ParameterDirection.Output));

        //                try
        //                {
        //                    //context.Database.Connection.Open();
        //                    cmd.ExecuteNonQuery();
        //                    result = cmd.Parameters["Hiddencolumn"].Value.ToString();
        //                    DropdownColumn = cmd.Parameters["DROPDOWNCOLUMN"].Value.ToString();
        //                }
        //                catch (Exception ex)
        //                {
        //                    ErrorLog lgerr = new ErrorLog();
        //                    lgerr.LogErrorInTextFile(ex);
        //                    SentErrorMail.SentEmailtoError("InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //                }
        //                finally
        //                {
        //                    context.Database.Connection.Close();
        //                }
        //            }
        //        }
        //    }
        //}
        //public DataSet GetDataSetValueForEdit(string selectCommand, int configid, string documentId, string p_EffectiveToDate)
        //{
        //    var result = new DataSet();
        //    using (var context = new SPONGE_Context())
        //    {
        //        using (var transaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted))
        //        {
        //            using (var cmd = context.Database.Connection.CreateCommand())
        //            {
        //                cmd.Transaction = transaction.UnderlyingTransaction;
        //                //var cmd = context.Database.Connection.CreateCommand();
        //                cmd.CommandText = selectCommand;
        //                cmd.CommandType = CommandType.StoredProcedure;

        //                cmd.Parameters.Add(CreateOracleParameter(configid, "p_ConfigID"));
        //                cmd.Parameters.Add(CreateOracleParameter(documentId, "p_document_ID"));
        //                cmd.Parameters.Add(CreateOracleParameter(p_EffectiveToDate, "p_EffectiveToDate"));
        //                cmd.Parameters.Add(CreateOracleParameter("Config_C"));

        //                try
        //                {
        //                    //  context.Database.Connection.Open();
        //                    var dtDataTable = new DataTable();
        //                    var reader = cmd.ExecuteReader();
        //                    do
        //                    {
        //                        dtDataTable.Load(reader);
        //                        result.Tables.Add(dtDataTable);

        //                    } while (!reader.IsClosed);
        //                }
        //                catch (Exception ex)
        //                {
        //                    ErrorLog lgerr = new ErrorLog();
        //                    lgerr.LogErrorInTextFile(ex);
        //                    SentErrorMail.SentEmailtoError("InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //                }
        //                finally
        //                {
        //                    context.Database.Connection.Close();
        //                }
        //            }
        //            return result;
        //        }
        //    }
        //}
        public DataSet GetDataSetValueForEditBatchjob(string selectCommand, int configid, string documentId, string p_EffectiveToDate)
        {
            var result = new DataSet();

            using (var context = new SPONGE_Context())
            {
                using (var cmd = context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = selectCommand;
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@p_ConfigID", configid));
                    cmd.Parameters.Add(new SqlParameter("@p_document_ID", documentId));
                    cmd.Parameters.Add(new SqlParameter("@p_EffectiveToDate", p_EffectiveToDate));
                    cmd.CommandTimeout = 180;
                    try
                    {
                        context.Database.OpenConnection();

                        using (var reader = cmd.ExecuteReader())
                        {
                            do
                            {
                                var dtDataTable = new DataTable();
                                dtDataTable.Load(reader);
                                result.Tables.Add(dtDataTable);
                            } while (!reader.IsClosed);
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLog lgerr = new ErrorLog();

                        lgerr.BatchJobLogErrorInTextFile(ex);
                    }
                    finally
                    {
                        context.Database.CloseConnection();
                    }
                }
            }

            return result;
        }
        public DataSet GetDataSetValueForBatchJob(string selectCommand, int configid, string documentId, string p_EffectiveToDate)
        {
            var dataSet = new DataSet();

            using (var context = new SPONGE_Context())
            {
                using (var cmd = context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = selectCommand;
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@p_ConfigID", configid));
                    cmd.Parameters.Add(new SqlParameter("@p_EffectiveToDate", p_EffectiveToDate));
                    cmd.CommandTimeout = 180;

                    try
                    {
                        context.Database.OpenConnection();

                        using (var result = cmd.ExecuteReader())
                        {
                            var dtDataTable = new DataTable();

                            dtDataTable.Load(result);
                            dataSet.Tables.Add(dtDataTable);
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLog lgerr = new ErrorLog();

                        lgerr.BatchJobLogErrorInTextFile(ex);
                    }
                    finally
                    {
                        context.Database.CloseConnection();
                    }
                }
            }

            return dataSet;
        }
        //public DataSet GetDataSetValue(string selectCommand, int configid, string documentId)
        //{
        //    var result = new DataSet();
        //    using (var context = new SPONGE_Context())
        //    {
        //        using (var transaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted))
        //        {
        //            using (var cmd = context.Database.Connection.CreateCommand())
        //            {
        //                cmd.Transaction = transaction.UnderlyingTransaction;
        //                //var cmd = context.Database.Connection.CreateCommand();
        //                cmd.CommandText = selectCommand;
        //                cmd.CommandType = CommandType.StoredProcedure;

        //                cmd.Parameters.Add(CreateOracleParameter(configid, "p_ConfigID"));
        //                cmd.Parameters.Add(CreateOracleParameter(documentId, "p_Document_ID"));
        //                cmd.Parameters.Add(CreateOracleParameter("Config_C"));

        //                try
        //                {
        //                    // context.Database.Connection.Open();
        //                    var dtDataTable = new DataTable();
        //                    var reader = cmd.ExecuteReader();
        //                    do
        //                    {
        //                        dtDataTable.Load(reader);
        //                        result.Tables.Add(dtDataTable);

        //                    } while (!reader.IsClosed);
        //                }
        //                catch (Exception ex)
        //                {
        //                    ErrorLog lgerr = new ErrorLog();
        //                    lgerr.LogErrorInTextFile(ex);
        //                    SentErrorMail.SentEmailtoError("InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //                }
        //                finally
        //                {
        //                    context.Database.Connection.Close();
        //                }
        //            }
        //            return result;
        //        }
        //    }
        //}
        //public DataSet ViewDataSetValue(string selectCommand, int configid, string documentId)
        //{
        //    var result = new DataSet();
        //    using (var context = new SPONGE_Context())
        //    {
        //        using (var transaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted))
        //        {
        //            using (var cmd = context.Database.Connection.CreateCommand())
        //            {
        //                cmd.Transaction = transaction.UnderlyingTransaction;
        //                //var cmd = context.Database.Connection.CreateCommand();
        //                cmd.CommandText = selectCommand;
        //                cmd.CommandType = CommandType.StoredProcedure;

        //                cmd.Parameters.Add(CreateOracleParameter(configid, "p_ConfigID"));
        //                cmd.Parameters.Add(CreateOracleParameter(documentId, "p_Document_ID"));
        //                cmd.Parameters.Add(CreateOracleParameter("Config_C"));

        //                try
        //                {
        //                    // context.Database.Connection.Open();
        //                    var dtDataTable = new DataTable();
        //                    var reader = cmd.ExecuteReader();
        //                    do
        //                    {
        //                        dtDataTable.Load(reader);
        //                        result.Tables.Add(dtDataTable);

        //                    } while (!reader.IsClosed);
        //                }
        //                catch (Exception ex)
        //                {
        //                    ErrorLog lgerr = new ErrorLog();
        //                    lgerr.LogErrorInTextFile(ex);
        //                    SentErrorMail.SentEmailtoError("InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);

        //                }
        //                finally
        //                {
        //                    context.Database.Connection.Close();
        //                }
        //            }
        //            return result;

        //        }
        //    }
        //}
        //public DataSet GetDataSetValue(string selectCommand, int configid, int templateId)
        //{
        //    var result = new DataSet();
        //    using (var context = new SPONGE_Context())
        //    {
        //        using (var transaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted))
        //        {
        //            using (var cmd = context.Database.Connection.CreateCommand())
        //            {
        //                cmd.Transaction = transaction.UnderlyingTransaction;
        //                //var cmd = context.Database.Connection.CreateCommand();
        //                cmd.CommandText = selectCommand;
        //                cmd.CommandType = CommandType.StoredProcedure;

        //                cmd.Parameters.Add(CreateOracleParameter(configid, "p_ConfigID"));
        //                cmd.Parameters.Add(CreateOracleParameter(templateId, "p_Template_ID"));
        //                cmd.Parameters.Add(CreateOracleParameter("Config_C"));

        //                try
        //                {
        //                    //  context.Database.Connection.Open();
        //                    var dtDataTable = new DataTable();
        //                    var reader = cmd.ExecuteReader();
        //                    do
        //                    {
        //                        dtDataTable.Load(reader);
        //                        result.Tables.Add(dtDataTable);

        //                    } while (!reader.IsClosed);
        //                }
        //                catch (Exception ex)
        //                {
        //                    ErrorLog lgerr = new ErrorLog();
        //                    lgerr.LogErrorInTextFile(ex);
        //                    SentErrorMail.SentEmailtoError("InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //                }
        //                finally
        //                {
        //                    context.Database.Connection.Close();
        //                }
        //            }
        //            return result;
        //        }
        //    }
        //}
        public string GetDataSetValueCheck(string selectCommand, int configid, string fortime, string ontime, out int TemplateId)
        {
            var result = new DataSet();
            string documentid = "";
            TemplateId = 0;
            using (var context = new SPONGE_Context())
            {
                using (var cmd = context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();
                    cmd.CommandText = selectCommand;
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@p_configID", configid));
                    cmd.Parameters.Add(new SqlParameter("@p_fortime", fortime));
                    cmd.Parameters.Add(new SqlParameter("@p_ontime", ontime));
                    var parameter2 = new SqlParameter("@p_id", SqlDbType.VarChar, 2000) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(parameter2);
                    var parameter3 = new SqlParameter("@p_Template_id", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(parameter3);
                    try
                    {
                        if (context.Database.GetDbConnection().State == ConnectionState.Closed)
                            context.Database.GetDbConnection().Open();

                        cmd.ExecuteNonQuery();

                        documentid = parameter2.Value.ToString();
                        TemplateId = Convert.ToInt32(parameter3.Value);
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        if (context.Database.GetDbConnection().State == ConnectionState.Open)
                            context.Database.GetDbConnection().Close();
                    }
                }
            }
            return documentid;
        }
        //public OracleParameter CreateOracleParameter(int value, string parameterName)
        //{
        //    OracleParameter param = new OracleParameter();
        //    param.ParameterName = parameterName;
        //    param.OracleDbType = OracleDbType.Int16;
        //    param.Direction = ParameterDirection.Input;
        //    param.Value = value;
        //    return param;
        //}

        //public OracleParameter CreateOracleParameter(byte[] value, string parameterName)
        //{
        //    OracleParameter param = new OracleParameter();
        //    param.ParameterName = parameterName;
        //    param.OracleDbType = OracleDbType.Raw;
        //    param.Direction = ParameterDirection.Input;
        //    param.Value = value;
        //    return param;
        //}
        //public OracleParameter CreateOracleParameter(string value, string parameterName)
        //{
        //    OracleParameter param = new OracleParameter();
        //    param.ParameterName = parameterName;
        //    param.OracleDbType = OracleDbType.Varchar2;
        //    param.Direction = ParameterDirection.Input;
        //    param.Value = value;
        //    return param;
        //}

        //public OracleParameter CreateOracleParameter(string parameterName, ParameterDirection direction)
        //{
        //    OracleParameter param = new OracleParameter();
        //    param.ParameterName = parameterName;
        //    param.OracleDbType = OracleDbType.Varchar2;
        //    param.Size = 4000;
        //    param.Direction = direction;
        //    return param;
        //}

        //public OracleParameter CreateOracleParameter(string parameterName)
        //{
        //    OracleParameter parameter2 = new OracleParameter();
        //    parameter2.ParameterName = parameterName;
        //    parameter2.Direction = ParameterDirection.Output;
        //    parameter2.OracleDbType = OracleDbType.RefCursor;
        //    return parameter2;
        //}


        //public DataSet GetDataSetValueDT(string selectCommand, decimal? subjectAreaId, string user_id)
        //{
        //    var result = new DataSet();
        //    using (var context = new SPONGE_Context())
        //    {
        //        using (var transaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted))
        //        {
        //            using (var cmd = context.Database.Connection.CreateCommand())
        //            {
        //                cmd.Transaction = transaction.UnderlyingTransaction;
        //                //var cmd = context.Database.Connection.CreateCommand();
        //                cmd.CommandText = selectCommand;
        //                cmd.CommandType = CommandType.StoredProcedure;

        //                OracleParameter param = new OracleParameter();
        //                param.ParameterName = "p_subjectAreaID";
        //                param.OracleDbType = OracleDbType.Int16;
        //                param.Direction = ParameterDirection.Input;
        //                param.Value = Convert.ToInt16(subjectAreaId);
        //                OracleParameter param2 = new OracleParameter();
        //                param2.ParameterName = "p_userID";
        //                param2.OracleDbType = OracleDbType.Varchar2;
        //                param2.Direction = ParameterDirection.Input;
        //                param2.Value = user_id;
        //                OracleParameter parameter2 = new OracleParameter();
        //                parameter2.ParameterName = "Config_C";
        //                parameter2.Direction = ParameterDirection.Output;
        //                parameter2.OracleDbType = OracleDbType.RefCursor;



        //                cmd.Parameters.Add(param);
        //                cmd.Parameters.Add(param2);
        //                cmd.Parameters.Add(parameter2);

        //                try
        //                {
        //                    // context.Database.Connection.Open();
        //                    var dtDataTable = new DataTable();
        //                    var reader = cmd.ExecuteReader();
        //                    do
        //                    {
        //                        dtDataTable.Load(reader);
        //                        result.Tables.Add(dtDataTable);

        //                    } while (!reader.IsClosed);
        //                }
        //                catch (Exception ex)
        //                {
        //                    ErrorLog lgerr = new ErrorLog();
        //                    lgerr.LogErrorInTextFile(ex);
        //                    SentErrorMail.SentEmailtoError("InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //                }
        //                finally
        //                {
        //                    context.Database.Connection.Close();
        //                }
        //            }
        //            return result;

        //        }
        //    }
        //}

        public List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();
            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                    {
                        if (pro.Name[0] == 'N')
                        {
                            decimal d = 0;
                            string columnValue = Convert.ToString(dr[column.ColumnName]);
                            if (!Decimal.TryParse(columnValue, out d))
                            {
                                PropertyInfo p = temp.GetProperties().FirstOrDefault(s => s.Name.Equals(pro.Name + "ControlType"));
                                p.SetValue(obj, columnValue, null);
                                continue;
                            }


                        }

                        if (pro.Name[0] == 'V')
                        {
                            string s = string.Empty;
                            string columnValue = Convert.ToString(dr[column.ColumnName]);
                            if (columnValue == "TEXTBOX" || columnValue == "DROPDOWN" || columnValue == "DATE")
                            {
                                PropertyInfo p = temp.GetProperties().FirstOrDefault(c => c.Name.Equals(pro.Name + "ControlType"));
                                p.SetValue(obj, columnValue, null);
                                continue;
                            }

                        }


                        if (pro.Name[0] == 'D')
                        {
                            DateTime d = new DateTime();
                            string columnValue = Convert.ToString(dr[column.ColumnName]);
                            if (!DateTime.TryParse(columnValue, out d))
                            {
                                PropertyInfo p = temp.GetProperties().FirstOrDefault(s => s.Name.Equals(pro.Name + "ControlType"));
                                p.SetValue(obj, columnValue, null);
                                continue;
                            }
                        }

                        if (!(dr.IsNull(column.ColumnName)))
                            pro.SetValue(obj, dr[column.ColumnName], null);

                    }
                    else
                        continue;
                }
            }
            return obj;
        }
        public static List<T> BindList<T>(DataTable dt)
        {
            // Example 2: Your case
            // Get all public fields
            var fields = typeof(T).GetFields();

            List<T> lst = new List<T>();

            foreach (DataRow dr in dt.Rows)
            {
                // Create the object of T
                var ob = Activator.CreateInstance<T>();

                foreach (var fieldInfo in fields)
                {
                    foreach (DataColumn dc in dt.Columns)
                    {
                        // Matching the columns with fields
                        if (fieldInfo.Name == dc.ColumnName)
                        {
                            Type type = fieldInfo.FieldType;

                            // Get the value from the datatable cell
                            object value = GetValue(dr[dc.ColumnName], type);

                            // Set the value into the object
                            fieldInfo.SetValue(ob, value);
                            break;
                        }
                    }
                }

                lst.Add(ob);
            }

            return lst;
        }
        static object GetValue(object ob, Type targetType)
        {
            if (targetType == null)
            {
                return null;
            }
            else if (targetType == typeof(String))
            {
                return ob + "";
            }
            else if (targetType == typeof(int))
            {
                int i = 0;
                int.TryParse(ob + "", out i);
                return i;
            }
            else if (targetType == typeof(short))
            {
                short i = 0;
                short.TryParse(ob + "", out i);
                return i;
            }
            else if (targetType == typeof(long))
            {
                long i = 0;
                long.TryParse(ob + "", out i);
                return i;
            }
            else if (targetType == typeof(ushort))
            {
                ushort i = 0;
                ushort.TryParse(ob + "", out i);
                return i;
            }
            else if (targetType == typeof(uint))
            {
                uint i = 0;
                uint.TryParse(ob + "", out i);
                return i;
            }
            else if (targetType == typeof(ulong))
            {
                ulong i = 0;
                ulong.TryParse(ob + "", out i);
                return i;
            }
            else if (targetType == typeof(double))
            {
                double i = 0;
                double.TryParse(ob + "", out i);
                return i;
            }
            else if (targetType == typeof(DateTime))
            {
                DateTime i = DateTime.MinValue;
                DateTime.TryParse(ob + "", out i);
                return i;
            }
            else if (targetType == typeof(bool))
            {
                Boolean b = false;
                Boolean.TryParse(ob + "", out b);
                return b;
            }
            else if (targetType == typeof(decimal))
            {
                Decimal d = Decimal.Zero;
                Decimal.TryParse(ob + "", out d);
                return d;
            }
            else if (targetType == typeof(float))
            {
                float d = 0;
                float.TryParse(ob + "", out d);
                return d;
            }
            else if (targetType == typeof(byte))
            {

            }
            else if (targetType == typeof(sbyte))
            {
                // do the parsing here...
            }


            return ob;
        }
        public string InsertDataSetValueStringParam(string selectCommand, string dynamicsqlquery)
        {
            string result = string.Empty;
            SPONGE_Context _context = new();
            try
            {
                var connection = _context.Database.GetDbConnection();

                using (var command = new SqlCommand(selectCommand, (SqlConnection)connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add the input parameter.
                    command.Parameters.Add(new SqlParameter("@p_dynamicsqlquery", dynamicsqlquery));

                    // Add the output parameter.
                    var outputParameter = new SqlParameter
                    {
                        ParameterName = "@Config_C",
                        SqlDbType = SqlDbType.NVarChar,
                        Size = -1,  // For nvarchar(max)
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(outputParameter);

                    _context.Database.OpenConnection();
                    command.ExecuteNonQuery();

                    result = outputParameter.Value.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                _context.Database.CloseConnection();
            }
            return result;
        }
        public DataSet GetDataSetValueStringParam(string selectCommand, string dynamicSqlQuery)
        {
            var result = new DataSet();
            using (var context = new SPONGE_Context())
            {
                using (var cmd = context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.CommandText = selectCommand;
                    cmd.CommandType = CommandType.StoredProcedure;

                    var param = new SqlParameter("@dynamicSqlQuery", SqlDbType.NVarChar)
                    {
                        Direction = ParameterDirection.Input,
                        Value = dynamicSqlQuery
                    };

                    cmd.Parameters.Add(param);

                    try
                    {
                        context.Database.OpenConnection();
                        using (var reader = cmd.ExecuteReader())
                        {
                            var dtDataTable = new DataTable();
                            dtDataTable.Load(reader);
                            result.Tables.Add(dtDataTable);
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLog lgerr = new ErrorLog();
                        lgerr.LogErrorInTextFile(ex);
                        //SentErrorMail.SendEmailToError("InnerException: " + ex.InnerException?.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
                    }
                }
            }
            return result;
        }
        //public void ExecuteDynamicQuery(string selectCommand, string dynamicsqlquery)
        //{
        //    var result = new DataSet();
        //    using (var context = new SPONGE_Context())
        //    {
        //        using (var transaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted))
        //        {
        //            using (var cmd = context.Database.Connection.CreateCommand())
        //            {
        //                cmd.Transaction = transaction.UnderlyingTransaction;
        //                //var cmd = context.Database.Connection.CreateCommand();
        //                cmd.CommandText = selectCommand;
        //                cmd.CommandType = CommandType.StoredProcedure;
        //                OracleParameter param = new OracleParameter();
        //                param.ParameterName = "p_dynamicsqlquery";
        //                param.OracleDbType = OracleDbType.Varchar2;
        //                param.Direction = ParameterDirection.Input;
        //                param.Value = dynamicsqlquery;

        //                cmd.Parameters.Add(param);
        //                try
        //                {
        //                    cmd.ExecuteNonQuery();
        //                }
        //                catch (Exception ex)
        //                {
        //                    ErrorLog lgerr = new ErrorLog();
        //                    lgerr.LogErrorInTextFile(ex);
        //                    SentErrorMail.SentEmailtoError("InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //                }
        //                finally
        //                {
        //                    context.Database.Connection.Close();
        //                }
        //            }
        //        }
        //    }
        //}
        public void DeleteUsingDynamicQuery(string selectCommand, string dynamicsqlquery)
        {
            using (var context = new SPONGE_Context())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    using (var cmd = context.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.Transaction = transaction.GetDbTransaction();
                        cmd.CommandText = selectCommand;
                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlParameter param = new SqlParameter();
                        param.ParameterName = "@p_dynamicsqlquery";
                        param.SqlDbType = SqlDbType.NVarChar;
                        param.Direction = ParameterDirection.Input;
                        param.Value = dynamicsqlquery;

                        cmd.Parameters.Add(param);
                        try
                        {
                            cmd.ExecuteNonQuery();
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            ErrorLog lgerr = new ErrorLog();
                            lgerr.LogErrorInTextFile(ex);
                            //SentErrorMail.SentEmailtoError("InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
                        }
                        finally
                        {
                            context.Database.CloseConnection();
                        }
                    }
                }
            }
        }
        //public void UpdateWhereClause(string selectCommand, string whereclause, decimal config_id)
        //{
        //    var result = new DataSet();
        //    using (var context = new SPONGE_Context())
        //    {
        //        using (var transaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted))
        //        {
        //            using (var cmd = context.Database.Connection.CreateCommand())
        //            {
        //                cmd.Transaction = transaction.UnderlyingTransaction;
        //                //var cmd = context.Database.Connection.CreateCommand();
        //                cmd.CommandText = selectCommand;
        //                cmd.CommandType = CommandType.StoredProcedure;

        //                OracleParameter param2 = new OracleParameter();
        //                param2.ParameterName = "p_cid";
        //                param2.OracleDbType = OracleDbType.Int32;
        //                param2.Direction = ParameterDirection.Input;
        //                param2.Value = config_id;

        //                OracleParameter param = new OracleParameter();
        //                param.ParameterName = "p_dynamicsqlquery";
        //                param.OracleDbType = OracleDbType.Clob;
        //                param.Direction = ParameterDirection.Input;
        //                param.Value = whereclause;

        //                cmd.Parameters.Add(param2);
        //                cmd.Parameters.Add(param);

        //                try
        //                {
        //                    context.Database.Connection.Open();
        //                    cmd.ExecuteNonQuery();
        //                }
        //                catch (Exception ex)
        //                {

        //                }
        //                finally
        //                {
        //                    context.Database.Connection.Close();
        //                }
        //            }
        //        }
        //    }
        //}

    }

    public class DeleteDBValues : IDisposable
    {
        void IDisposable.Dispose()
        {

        }
        //    public void DeleteUsingDynamicQuery(string selectCommand, string dynamicsqlquery)
        //    {
        //        var result = new DataSet();
        //        using (var context = new SPONGE_Context())
        //        {
        //            using (var transaction = context.Database.BeginTransaction(IsolationLevel.ReadCommitted))
        //            {
        //                using (var cmd = context.Database.Connection.CreateCommand())
        //                {
        //                    cmd.Transaction = transaction.UnderlyingTransaction;
        //                    cmd.CommandText = selectCommand;
        //                    cmd.CommandType = CommandType.StoredProcedure;
        //                    OracleParameter param = new OracleParameter();
        //                    param.ParameterName = "p_dynamicsqlquery";
        //                    param.OracleDbType = OracleDbType.Varchar2;
        //                    param.Direction = ParameterDirection.Input;
        //                    param.Value = dynamicsqlquery;

        //                    cmd.Parameters.Add(param);
        //                    try
        //                    {
        //                        cmd.ExecuteNonQuery();
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        ErrorLog lgerr = new ErrorLog();
        //                        lgerr.LogErrorInTextFile(ex);
        //                        SentErrorMail.SentEmailtoError("InnerException: " + ex.InnerException.ToString() + " StackTrace: " + ex.StackTrace.ToString() + " Message" + ex.Message);
        //                    }
        //                    finally
        //                    {
        //                        context.Database.Connection.Close();
        //                    }
        //                }
        //            }

        //        }
        //    }
        //}
    }
}
