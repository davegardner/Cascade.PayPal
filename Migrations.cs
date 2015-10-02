using System;
using System.Collections.Generic;
using System.Data;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Cascade.Paypal
{
    public class Migrations : DataMigrationImpl
    {

        public int Create()
        {
         

            // Creating table PaypalExpressRecord
            SchemaBuilder.CreateTable("PaypalExpressRecord", table => table
                .ContentPartRecord()
                .Column("Sandbox", DbType.Boolean)
                .Column("SandboxApiBaseUrl", DbType.String)
                .Column("LiveApiBaseUrl", DbType.String)
                .Column("SandboxAuthorizationBaseUrl", DbType.String)
                .Column("LiveAuthorizationBaseUrl", DbType.String)
                .Column("PaypalUser", DbType.String)
                .Column("PaypalPwd", DbType.String)
                .Column("PaypalSignature", DbType.String)
                .Column("Version", DbType.String)
                .Column("Currency", DbType.String)

            );

            return 1;
        }

        //public int UpdateFrom1()
        //{
        //    //SchemaBuilder.AlterTable("PaypalTransactionRecord", table=> {
        //    //    table.AddColumn("Timestamp", DbType.DateTime);
        //    //    table.AddColumn("ErrorCodes", DbType.String);
        //    //    table.AddColumn("ShortMessages", DbType.String);
        //    //    table.AddColumn("LongMessages", DbType.String);
        //    //});
        //    return 2;
        //}
    }
}