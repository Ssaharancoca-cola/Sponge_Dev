﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using DAL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace DAL.Models
{
    public partial class SPONGE_Context
    {
        private ISPONGE_ContextProcedures _procedures;

        public virtual ISPONGE_ContextProcedures Procedures
        {
            get
            {
                if (_procedures is null) _procedures = new SPONGE_ContextProcedures(this);
                return _procedures;
            }
            set
            {
                _procedures = value;
            }
        }

        public ISPONGE_ContextProcedures GetProcedures()
        {
            return Procedures;
        }

        protected void OnModelCreatingGeneratedProcedures(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SP_GET_MPP_DIMENSIONSResult>().HasNoKey().ToView(null);
            modelBuilder.Entity<SP_GET_MPP_MASTERSResult>().HasNoKey().ToView(null);
            modelBuilder.Entity<SP_GETMASTEREMAILResult>().HasNoKey().ToView(null);
        }
    }

    public partial class SPONGE_ContextProcedures : ISPONGE_ContextProcedures
    {
        private readonly SPONGE_Context _context;

        public SPONGE_ContextProcedures(SPONGE_Context context)
        {
            _context = context;
        }

        public virtual async Task<List<SP_GET_MPP_DIMENSIONSResult>> SP_GET_MPP_DIMENSIONSAsync(OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default)
        {
            var parameterreturnValue = new SqlParameter
            {
                ParameterName = "returnValue",
                Direction = System.Data.ParameterDirection.Output,
                SqlDbType = System.Data.SqlDbType.Int,
            };

            var sqlParameters = new []
            {
                parameterreturnValue,
            };
            var _ = await _context.SqlQueryAsync<SP_GET_MPP_DIMENSIONSResult>("EXEC @returnValue = [dbo].[SP_GET_MPP_DIMENSIONS]", sqlParameters, cancellationToken);

            returnValue?.SetValue(parameterreturnValue.Value);

            return _;
        }

        public virtual async Task<List<SP_GET_MPP_MASTERSResult>> SP_GET_MPP_MASTERSAsync(string Dimension_Name, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default)
        {
            var parameterreturnValue = new SqlParameter
            {
                ParameterName = "returnValue",
                Direction = System.Data.ParameterDirection.Output,
                SqlDbType = System.Data.SqlDbType.Int,
            };

            var sqlParameters = new []
            {
                new SqlParameter
                {
                    ParameterName = "Dimension_Name",
                    Size = -1,
                    Value = Dimension_Name ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.VarChar,
                },
                parameterreturnValue,
            };
            var _ = await _context.SqlQueryAsync<SP_GET_MPP_MASTERSResult>("EXEC @returnValue = [dbo].[SP_GET_MPP_MASTERS] @Dimension_Name", sqlParameters, cancellationToken);

            returnValue?.SetValue(parameterreturnValue.Value);

            return _;
        }

        public virtual async Task<List<SP_GETMASTEREMAILResult>> SP_GETMASTEREMAILAsync(int? p_ConfigID, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default)
        {
            var parameterreturnValue = new SqlParameter
            {
                ParameterName = "returnValue",
                Direction = System.Data.ParameterDirection.Output,
                SqlDbType = System.Data.SqlDbType.Int,
            };

            var sqlParameters = new []
            {
                new SqlParameter
                {
                    ParameterName = "p_ConfigID",
                    Value = p_ConfigID ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.Int,
                },
                parameterreturnValue,
            };
            var _ = await _context.SqlQueryAsync<SP_GETMASTEREMAILResult>("EXEC @returnValue = [dbo].[SP_GETMASTEREMAIL] @p_ConfigID", sqlParameters, cancellationToken);

            returnValue?.SetValue(parameterreturnValue.Value);

            return _;
        }
    }
}
