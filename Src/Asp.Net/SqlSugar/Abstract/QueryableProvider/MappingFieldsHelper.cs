﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SqlSugar 
{
    internal class MappingFieldsHelper<T> 
    {
        public SqlSugarProvider Context { get; set; }
        public EntityInfo NavEntity { get;  set; }
        public EntityInfo RootEntity { get;  set; }

        public MappingFieldsInfo GetMappings(Expression thisFiled, Expression  mappingFiled)
        {
            MappingFieldsInfo mappingFields=new MappingFieldsInfo();
            var pkName = "";
            if ((mappingFiled as LambdaExpression).Body is UnaryExpression)
            {
                pkName = (((mappingFiled as LambdaExpression).Body as UnaryExpression).Operand as MemberExpression).Member.Name;
            }
            else
            {
                pkName = ((mappingFiled as LambdaExpression).Body as MemberExpression).Member.Name;
            }
            return mappingFields;
        }

        public List<IConditionalModel> GetMppingSql(List<T> list, List<MappingFieldsExpression>  mappingFieldsExpressions)
        {
            List<IConditionalModel> conditionalModels = new List<IConditionalModel>();
            foreach (var model in list) 
            {
                var clist = new List<KeyValuePair<WhereType, ConditionalModel>>();
                foreach (var item in mappingFieldsExpressions)
                {
                    InitMappingFieldsExpression(item);
                    clist.Add(new KeyValuePair<WhereType, ConditionalModel>(WhereType.Or, new ConditionalModel()
                    {
                        FieldName = item.LeftEntityColumn.DbColumnName,
                        ConditionalType = ConditionalType.Equal,
                        FieldValue = item.RightEntityColumn.PropertyInfo.GetValue(model).ObjToString(),
                        CSharpTypeName = item.RightEntityColumn.PropertyInfo.PropertyType.Name
                    }));
                }
                conditionalModels.Add(new ConditionalCollections() { 
                  ConditionalList= clist
                });
            }
            return conditionalModels;
        }

        public void SetChildList(EntityColumnInfo navColumnInfo,object item,List<object> list, List<MappingFieldsExpression> mappingFieldsExpressions)
        {
            if (item != null) 
            {
                //var expable =Expressionable.Create<object>();
                foreach (var field in mappingFieldsExpressions) 
                {
                    InitMappingFieldsExpression(field);
                }
                var setList =new List<object>();
                var count = mappingFieldsExpressions.Count;
                if (count == 1)
                {
                    setList=list.Where(it => GetWhereByIndex(item, mappingFieldsExpressions, it, 0)).ToList();
                }
                else if (count == 2)
                {
                    setList = list.Where(it =>
                     GetWhereByIndex(item, mappingFieldsExpressions, it, 0) ||
                     GetWhereByIndex(item, mappingFieldsExpressions, it, 1)
                    ).ToList();
                }
                else if (count == 3)
                {
                    setList = list.Where(it =>
                     GetWhereByIndex(item, mappingFieldsExpressions, it, 0) ||
                     GetWhereByIndex(item, mappingFieldsExpressions, it, 1) ||
                     GetWhereByIndex(item, mappingFieldsExpressions, it, 2)
                    ).ToList();
                }
                else if (count == 4)
                {
                    setList = list.Where(it =>
                     GetWhereByIndex(item, mappingFieldsExpressions, it, 0) ||
                     GetWhereByIndex(item, mappingFieldsExpressions, it, 1) ||
                     GetWhereByIndex(item, mappingFieldsExpressions, it, 2) ||
                     GetWhereByIndex(item, mappingFieldsExpressions, it, 3)
                    ).ToList();
                }
                else if (count == 5)
                {
                    setList = list.Where(it =>
                     GetWhereByIndex(item, mappingFieldsExpressions, it, 0) ||
                     GetWhereByIndex(item, mappingFieldsExpressions, it, 1) ||
                     GetWhereByIndex(item, mappingFieldsExpressions, it, 2) ||
                     GetWhereByIndex(item, mappingFieldsExpressions, it, 3) ||
                     GetWhereByIndex(item, mappingFieldsExpressions, it, 4)
                    ).ToList();
                }
                else 
                {
                    Check.ExceptionEasy("MappingField max value  is  5", "MappingField最大数量不能超过5");
                }
                //navColumnInfo.PropertyInfo.SetValue();
                var instance = Activator.CreateInstance(navColumnInfo.PropertyInfo.PropertyType, true);
                var ilist = instance as IList;
                foreach (var value in setList)
                {
                    ilist.Add(value);
                }
                navColumnInfo.PropertyInfo.SetValue(item,ilist);
            }
        }

        private static bool GetWhereByIndex(object item, List<MappingFieldsExpression> mappingFieldsExpressions, object it,int index)
        {
            return mappingFieldsExpressions[index].LeftEntityColumn.PropertyInfo.GetValue(it).ObjToString() == mappingFieldsExpressions[index].RightEntityColumn.PropertyInfo.GetValue(item).ObjToString();
        }

        private void InitMappingFieldsExpression(MappingFieldsExpression item)
        {
            var leftName = item.LeftName;
            var rightName = item.RightName;
            if (item.LeftEntityColumn == null)
            {
                item.LeftEntityColumn = this.NavEntity.Columns.FirstOrDefault(it => it.PropertyName == leftName);
            }
            if (item.RightEntityColumn == null)
            {
                item.RightEntityColumn = this.RootEntity.Columns.FirstOrDefault(it => it.PropertyName == rightName);
            }
        }

    }
    public class MappingFieldsInfo
    {
        public DbColumnInfo LeftColumn { get; set; }
        public DbColumnInfo RightColumn { get; set; }
    }
    public class MappingFieldsExpression
    {
        public Expression LeftColumnExpression { get; set; }
        public Expression RightColumnExpression { get; set; }
        public EntityColumnInfo LeftEntityColumn { get; set; }
        public EntityColumnInfo RightEntityColumn { get; set; }
        private string _LeftName;
        public string LeftName
        {
            get
            {
                if (_LeftName == null)
                {
                    _LeftName = ExpressionTool.GetMemberName(this.LeftColumnExpression);
                }
                return _LeftName;
            }
        }
        private string _RightName;
        public string RightName
        {
            get
            {
                if (_RightName == null)
                {
                    _RightName = ExpressionTool.GetMemberName(this.RightColumnExpression);
                }
                return _RightName;
            }
        }
    }
}
