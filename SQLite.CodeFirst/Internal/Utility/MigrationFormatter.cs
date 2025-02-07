﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Migrations.Model;
using System.Globalization;
using System.IO;

namespace SQLite.CodeFirst.Utility
{
    public static class MigrationFormatter
    {
        private const int DefaultStringMaxLength = 255;
        private const int DefaultNumericPrecision = 10;
        private const byte DefaultTimePrecision = 7;
        private const byte DefaultNumericScale = 0;

        /// <summary>
        /// Builds a column type
        /// </summary>
        /// <returns> SQL representing the data type. </returns>
        public static string BuildColumnType(DbProviderManifest providerManifest, ColumnModel column)
        {
            if (column == null)
                return string.Empty;

            return column.IsTimestamp ? "rowversion" : BuildPropertyType(providerManifest, column);
        }

        /// <summary>
        /// Builds a SQL property type fragment from the specified <see cref="ColumnModel"/>.
        /// </summary>
        public static string BuildPropertyType(DbProviderManifest providerManifest, PropertyModel column)
        {
            if (providerManifest == null || column == null)
                return string.Empty;

            var originalStoreType = column.StoreType;

            if (string.IsNullOrWhiteSpace(originalStoreType))
            {
                var typeUsage = providerManifest.GetStoreType(column.TypeUsage).EdmType;
                originalStoreType = typeUsage.Name;
            }

            var storeType = originalStoreType;

            const string maxSuffix = "(max)";

            if (storeType.EndsWith(maxSuffix, StringComparison.Ordinal))
                storeType = storeType.Substring(0, storeType.Length - maxSuffix.Length) + maxSuffix;

            if (originalStoreType.ToUpperInvariant() == "DECIMAL" || originalStoreType.ToUpperInvariant() == "NUMERIC")
                storeType += "(" + (column.Precision ?? DefaultNumericPrecision)
                                 + ", " + (column.Scale ?? DefaultNumericScale) + ")";
            else if (originalStoreType.ToUpperInvariant() == "DATETIME" ||
                     originalStoreType.ToUpperInvariant() == "TIME")
                storeType += "(" + (column.Precision ?? DefaultTimePrecision) + ")";
            else if (originalStoreType.ToUpperInvariant() == "BLOB" ||
                     originalStoreType.ToUpperInvariant() == "VARCHAR2" ||
                     originalStoreType.ToUpperInvariant() == "VARCHAR" ||
                     originalStoreType.ToUpperInvariant() == "CHAR" ||
                     originalStoreType.ToUpperInvariant() == "NVARCHAR" ||
                     originalStoreType.ToUpperInvariant() == "NVARCHAR2")
                storeType += "(" + (column.MaxLength ?? DefaultStringMaxLength) + ")";

            return storeType;
        }

        /// <summary>
        /// Gets an <see cref="IndentedTextWriter" /> object used to format SQL script.
        /// </summary>
        /// <returns> An empty text writer to use for SQL generation. </returns>
        public static IndentedTextWriter CreateIndentedTextWriter()
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);
            try
            {
                return new IndentedTextWriter(writer);
            }
            catch
            {
                writer.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Remove occurrences of "dbo." from the supplied string.
        /// </summary>
        public static string RemoveDbo(string str)
        {
            if (str == null)
                return string.Empty;

            return str.Replace("dbo.", string.Empty);
        }

        /// <summary>
        /// Surround with double-quotes Sqlite reserved words.
        /// </summary>
        public static string ReservedWord(string word)
        {
            if (word == null)
                return string.Empty;

            List<string> cultureList = new List<string>
            {
                "ABORT",
                "ACTION",
                "ADD",
                "AFTER",
                "ALL",
                "ALTER",
                "ANALYZE",
                "AND",
                "AS",
                "ASC",
                "ATTACH",
                "AUTOINCREMENT",
                "BEFORE",
                "BEGIN",
                "BETWEEN",
                "BY",
                "CASCADE",
                "CASE",
                "CAST",
                "CHECK",
                "COLLATE",
                "COLUMN",
                "COMMIT",
                "CONFLICT",
                "CONSTRAINT",
                "CREATE",
                "CROSS",
                "CURRENT_DATE",
                "CURRENT_TIME",
                "CURRENT_TIMESTAMP",
                "DATABASE",
                "DEFAULT",
                "DEFERRABLE",
                "DEFERRED",
                "DELETE",
                "DESC",
                "DETACH",
                "DISTINCT",
                "DROP",
                "EACH",
                "ELSE",
                "END",
                "ESCAPE",
                "EXCEPT",
                "EXCLUSIVE",
                "EXISTS",
                "EXPLAIN",
                "FAIL",
                "FOR",
                "FOREIGN",
                "FROM",
                "FULL",
                "GLOB",
                "GROUP",
                "HAVING",
                "IF",
                "IGNORE",
                "IMMEDIATE",
                "IN",
                "INDEX",
                "INDEXED",
                "INITIALLY",
                "INNER",
                "INSERT",
                "INSTEAD",
                "INTERSECT",
                "INTO",
                "IS",
                "ISNULL",
                "JOIN",
                "KEY",
                "LEFT",
                "LIKE",
                "LIMIT",
                "MATCH",
                "NATURAL",
                "NO",
                "NOT",
                "NOTNULL",
                "NULL",
                "OF",
                "OFFSET",
                "ON",
                "OR",
                "ORDER",
                "OUTER",
                "PLAN",
                "PRAGMA",
                "PRIMARY",
                "QUERY",
                "RAISE",
                "RECURSIVE",
                "REFERENCES",
                "REGEXP",
                "REINDEX",
                "RELEASE",
                "RENAME",
                "REPLACE",
                "RESTRICT",
                "RIGHT",
                "ROLLBACK",
                "ROW",
                "SAVEPOINT",
                "SELECT",
                "SET",
                "TABLE",
                "TEMP",
                "TEMPORARY",
                "THEN",
                "TO",
                "TRANSACTION",
                "TRIGGER",
                "UNION",
                "UNIQUE",
                "UPDATE",
                "USING",
                "VACUUM",
                "VALUES",
                "VIEW",
                "VIRTUAL",
                "WHEN",
                "WHERE",
                "WITH",
                "WITHOUT"
            };

            string elem = cultureList.Find(i => i == word.ToUpper(CultureInfo.InvariantCulture));

            if (elem != null)
            {
                return '"' + word + '"';
            }
            else { return word; }

            //switch (word.ToUpper(CultureInfo.InvariantCulture))
            //{
            //    case "ABORT":
            //    case "ACTION":
            //    case "ADD":
            //    case "AFTER":
            //    case "ALL":
            //    case "ALTER":
            //    case "ANALYZE":
            //    case "AND":
            //    case "AS":
            //    case "ASC":
            //    case "ATTACH":
            //    case "AUTOINCREMENT":
            //    case "BEFORE":
            //    case "BEGIN":
            //    case "BETWEEN":
            //    case "BY":
            //    case "CASCADE":
            //    case "CASE":
            //    case "CAST":
            //    case "CHECK":
            //    case "COLLATE":
            //    case "COLUMN":
            //    case "COMMIT":
            //    case "CONFLICT":
            //    case "CONSTRAINT":
            //    case "CREATE":
            //    case "CROSS":
            //    case "CURRENT_DATE":
            //    case "CURRENT_TIME":
            //    case "CURRENT_TIMESTAMP":
            //    case "DATABASE":
            //    case "DEFAULT":
            //    case "DEFERRABLE":
            //    case "DEFERRED":
            //    case "DELETE":
            //    case "DESC":
            //    case "DETACH":
            //    case "DISTINCT":
            //    case "DROP":
            //    case "EACH":
            //    case "ELSE":
            //    case "END":
            //    case "ESCAPE":
            //    case "EXCEPT":
            //    case "EXCLUSIVE":
            //    case "EXISTS":
            //    case "EXPLAIN":
            //    case "FAIL":
            //    case "FOR":
            //    case "FOREIGN":
            //    case "FROM":
            //    case "FULL":
            //    case "GLOB":
            //    case "GROUP":
            //    case "HAVING":
            //    case "IF":
            //    case "IGNORE":
            //    case "IMMEDIATE":
            //    case "IN":
            //    case "INDEX":
            //    case "INDEXED":
            //    case "INITIALLY":
            //    case "INNER":
            //    case "INSERT":
            //    case "INSTEAD":
            //    case "INTERSECT":
            //    case "INTO":
            //    case "IS":
            //    case "ISNULL":
            //    case "JOIN":
            //    case "KEY":
            //    case "LEFT":
            //    case "LIKE":
            //    case "LIMIT":
            //    case "MATCH":
            //    case "NATURAL":
            //    case "NO":
            //    case "NOT":
            //    case "NOTNULL":
            //    case "NULL":
            //    case "OF":
            //    case "OFFSET":
            //    case "ON":
            //    case "OR":
            //    case "ORDER":
            //    case "OUTER":
            //    case "PLAN":
            //    case "PRAGMA":
            //    case "PRIMARY":
            //    case "QUERY":
            //    case "RAISE":
            //    case "RECURSIVE":
            //    case "REFERENCES":
            //    case "REGEXP":
            //    case "REINDEX":
            //    case "RELEASE":
            //    case "RENAME":
            //    case "REPLACE":
            //    case "RESTRICT":
            //    case "RIGHT":
            //    case "ROLLBACK":
            //    case "ROW":
            //    case "SAVEPOINT":
            //    case "SELECT":
            //    case "SET":
            //    case "TABLE":
            //    case "TEMP":
            //    case "TEMPORARY":
            //    case "THEN":
            //    case "TO":
            //    case "TRANSACTION":
            //    case "TRIGGER":
            //    case "UNION":
            //    case "UNIQUE":
            //    case "UPDATE":
            //    case "USING":
            //    case "VACUUM":
            //    case "VALUES":
            //    case "VIEW":
            //    case "VIRTUAL":
            //    case "WHEN":
            //    case "WHERE":
            //    case "WITH":
            //    case "WITHOUT":
            //        return '"' + word + '"';

            //    default:
            //        return word;
            //}
        }

        public static string UniqueConflictText(AnnotationValues uniqueAnnotation)
        {
            if (uniqueAnnotation == null)
                return string.Empty;

            var uniqueText = Convert.ToString(uniqueAnnotation.NewValue, CultureInfo.InvariantCulture);

            if (uniqueText != null && !uniqueText.StartsWith("OnConflict:", StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            var actionText = uniqueText?.Remove(0, "OnConflict:".Length).Trim();
            if (!Enum.TryParse(actionText, out OnConflictAction action))
                return string.Empty;

            if (action == OnConflictAction.None)
                return string.Empty;

            return " ON CONFLICT " + action.ToString().ToUpperInvariant();
        }

        public static string CollateFunctionText(AnnotationValues collateAnnotation)
        {
            if (collateAnnotation == null)
                return string.Empty;

            var collateAttributeText = Convert.ToString(collateAnnotation.NewValue, CultureInfo.InvariantCulture);
            string collateFunction;
            string collateCustomFunction;

            if (collateAttributeText != null && collateAttributeText.IndexOf(':') > -1)
            {
                collateFunction = collateAttributeText.Substring(0, collateAttributeText.IndexOf(':'));
                collateCustomFunction = collateAttributeText.Remove(0, collateAttributeText.IndexOf(':') + 1).Trim();
            }
            else
            {
                collateFunction = collateAttributeText;
                collateCustomFunction = string.Empty;
            }

            if (!Enum.TryParse(collateFunction, out CollationFunction colatteFunctionType))
                return string.Empty;

            if (colatteFunctionType == CollationFunction.None)
            {
                return string.Empty;
            }

            return colatteFunctionType == CollationFunction.Custom
                ? " COLLATE " + collateCustomFunction
                : " COLLATE " + colatteFunctionType.ToString().ToUpperInvariant();
        }

    }
}
