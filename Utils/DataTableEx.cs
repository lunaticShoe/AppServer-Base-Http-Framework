﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AppServerBase.Utils
{
    public class DataTableEx : DataTable
    {
        public DataTableEx() : base()
        {

        }

        public DataTableEx(string tableName) : base(tableName)
        {

        }

        public DataTableEx(DataTable dataTable)
        {
            Assign(dataTable);
        }

        public DataTableEx(System.Runtime.Serialization.SerializationInfo info, 
            System.Runtime.Serialization.StreamingContext context) : base (info,context)
        {

        }
        public DataTableEx(string tableName, string tableNamespace) : base(tableName, tableNamespace)
        {
            
        }


        private object ChangeType(object value, Type conversion)
        {
            var t = conversion;

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }
                t = Nullable.GetUnderlyingType(t);
            }
            if (t.IsEnum)
                return Enum.ToObject(t, Convert.ToInt32(value));
            return Convert.ChangeType(value, t);
        }

        public List<T> GetObjects<T>()
        {
            List<T> objects = new List<T>();
            var props = typeof(T).GetProperties();
            foreach (DataRow row in Rows)
            {
                var item = Activator.CreateInstance<T>();
                foreach (DataColumn column in Columns)
                {
                    var prop = (from pr in props
                                where pr.GetCustomAttributes()?.Count() > 0
                                && (pr.GetCustomAttributes().ElementAt(0) is ColumnAttribute)
                                && (pr.GetCustomAttributes().ElementAt(0) as ColumnAttribute)
                                    .ColumnName == column.ColumnName
                                select pr).FirstOrDefault();

                    if (prop == null)
                        continue;

                    if (row[column] == DBNull.Value)
                    {
                        prop.SetValue(item, null);
                        continue;                        
                    }
                    //Convert.ChangeType(row[column],prop.PropertyType)
                    prop.SetValue(item, ChangeType(row[column], prop.PropertyType));    
                }
                objects.Add(item);
                //yield return item;
            }
            return objects;
        }

        public JArray GetJSONObjectArray<T>()
        {
            return JArray.FromObject(GetObjects<T>());
        }

        public bool CompareTo(DataTable dataTable)
        {
            if (dataTable == null)
                return false;

            if (this.Rows.Count != dataTable.Rows.Count || 
                this.Columns.Count != dataTable.Columns.Count)
                return false;

            for (int i = 0; i < this.Rows.Count; i++)
            {
                for (int c = 0; c < this.Columns.Count; c++)
                {
                    if (!Equals(this.Rows[i][c], dataTable.Rows[i][c]))
                        return false;
                }
            }
            return true;
        }
        
        public void Assign(DataTable dataTable)
        {
            Clear();

            foreach (DataColumn column in  dataTable.Columns)
            {
                this.Columns.Add(new DataColumn(column.ColumnName, column.DataType));
            }

            foreach (DataRow row in dataTable.Rows)
            {
                var newRow = NewRow();

                foreach (DataColumn column in dataTable.Columns)
                {
                    newRow[column.ColumnName] = row[column.ColumnName];
                }

                Rows.Add(newRow);
            }
        }
        
    }
}
