using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using CYQ.Data.SQL;

namespace CYQ.Data.Table
{
    /// <summary>
    /// 列结构修选项
    /// </summary>
    [Flags]
    public enum AlterOp
    {
        /// <summary>
        /// 默认不修改状态
        /// </summary>
        None = 0,
        /// <summary>
        /// 添加或修改状态
        /// </summary>
        AddOrModify = 1,
        /// <summary>
        /// 删除列状态
        /// </summary>
        Drop = 2,
        /// <summary>
        /// 重命名列状态
        /// </summary>
        Rename = 4
    }
    /// <summary>
    /// 单元结构属性
    /// </summary>
    public partial class MCellStruct
    {
        private MDataColumn _MDataColumn = null;
        /// <summary>
        /// 结构集合
        /// </summary>
        public MDataColumn MDataColumn
        {
            get
            {
                return _MDataColumn;
            }
            internal set
            {
                _MDataColumn = value;
            }
        }
        /// <summary>
        /// 是否对值进行格式校验
        /// </summary>
        //public bool IsCheckValue = true;
        /// <summary>
        /// 是否关键字
        /// </summary>
        public bool IsPrimaryKey = false;

        /// <summary>
        /// 是否唯一索引
        /// </summary>
        public bool IsUniqueKey = false;

        /// <summary>
        /// 是否外键
        /// </summary>
        public bool IsForeignKey = false;
        /// <summary>
        /// 外键表名
        /// </summary>
        public string FKTableName;

        /// <summary>
        /// 字段描述
        /// </summary>
        public string Description;
        /// <summary>
        /// 默认值
        /// </summary>
        public object DefaultValue;
        /// <summary>
        /// 是否允许为Null
        /// </summary>
        public bool IsCanNull;
        /// <summary>
        /// 是否自增加
        /// </summary>
        public bool IsAutoIncrement;
        /// <summary>
        /// 旧的列名（AlterOp为Rename时可用）
        /// </summary>
        public string OldName;
        private string _ColumnName = string.Empty;
        /// <summary>
        /// 列名
        /// </summary>
        public string ColumnName
        {
            get
            {
                return _ColumnName;
            }
            set
            {
                _ColumnName = value;
                if (_MDataColumn != null)
                {
                    _MDataColumn.IsColumnNameChanged = true;//列名已变更，存储索引也需要变更
                }
            }
        }
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName;
        private SqlDbType _SqlType;
        /// <summary>
        /// SqlDbType类型
        /// </summary>
        public SqlDbType SqlType
        {
            get
            {
                return _SqlType;
            }
            set
            {
                _SqlType = value;
                ValueType = DataType.GetType(_SqlType, DalType);
            }
        }
        /// <summary>
        /// 最大字节
        /// </summary>
        public int MaxSize;

        /// <summary>
        /// 精度（小数位）
        /// </summary>
        public short Scale;
        /// <summary>
        /// 原始的数据库字段类型名称
        /// </summary>
        internal string SqlTypeName;
        internal Type ValueType;
        private DalType dalType = DalType.None;
        internal DalType DalType
        {
            get
            {
                if (_MDataColumn != null)
                {
                    return _MDataColumn.dalType;
                }
                return dalType;
            }
        }
        private AlterOp _AlterOp = AlterOp.None;
        /// <summary>
        /// 列结构改变状态
        /// </summary>
        public AlterOp AlterOp
        {
            get { return _AlterOp; }
            set { _AlterOp = value; }
        }
        //内部使用的索引，在字段名为空时使用
        internal int ReaderIndex = -1;

        #region 构造函数
        internal MCellStruct(DalType dalType)
        {
            this.dalType = dalType;
        }
        public MCellStruct(string columnName, SqlDbType sqlType)
        {
            Init(columnName, sqlType, false, true, false, -1, null);
        }
        public MCellStruct(string columnName, SqlDbType sqlType, bool isAutoIncrement, bool isCanNull, int maxSize)
        {
            Init(columnName, sqlType, isAutoIncrement, isCanNull, false, maxSize, null);
        }
        internal void Init(string columnName, SqlDbType sqlType, bool isAutoIncrement, bool isCanNull, bool isPrimaryKey, int maxSize, object defaultValue)
        {
            ColumnName = columnName.Trim();
            SqlType = sqlType;
            IsAutoIncrement = isAutoIncrement;
            IsCanNull = isCanNull;
            MaxSize = maxSize;
            IsPrimaryKey = isPrimaryKey;
            DefaultValue = defaultValue;
        }
        internal void Load(MCellStruct ms)
        {
            ColumnName = ms.ColumnName;
            SqlType = ms.SqlType;
            IsAutoIncrement = ms.IsAutoIncrement;
            IsCanNull = ms.IsCanNull;
            MaxSize = ms.MaxSize;
            Scale = ms.Scale;
            IsPrimaryKey = ms.IsPrimaryKey;
            IsUniqueKey = ms.IsUniqueKey;
            IsForeignKey = ms.IsForeignKey;
            FKTableName = ms.FKTableName;
            SqlTypeName = ms.SqlTypeName;
            AlterOp = ms.AlterOp;

            if (ms.DefaultValue != null)
            {
                DefaultValue = ms.DefaultValue;
            }
            if (!string.IsNullOrEmpty(ms.Description))
            {
                Description = ms.Description;
            }
        }
        /// <summary>
        /// 克隆一个对象。
        /// </summary>
        /// <returns></returns>
        public MCellStruct Clone()
        {
            MCellStruct ms = new MCellStruct(dalType);
            ms.ColumnName = ColumnName;
            ms.SqlType = SqlType;
            ms.IsAutoIncrement = IsAutoIncrement;
            ms.IsCanNull = IsCanNull;
            ms.MaxSize = MaxSize;
            ms.Scale = Scale;
            ms.IsPrimaryKey = IsPrimaryKey;
            ms.IsUniqueKey = IsUniqueKey;
            ms.IsForeignKey = IsForeignKey;
            ms.FKTableName = FKTableName;
            ms.SqlTypeName = SqlTypeName;
            ms.DefaultValue = DefaultValue;
            ms.Description = Description;
            ms.MDataColumn = MDataColumn;
            ms.AlterOp = AlterOp;
            ms.TableName = TableName;
            return ms;
        }
        #endregion
    }

    public partial class MCellStruct // 扩展几个常用方法
    {
        /// <summary>
        /// 为列的所有行设置值
        /// </summary>
        public MCellStruct Set(object value)
        {
            Set(value, -1);
            return this;
        }
        public MCellStruct Set(object value, int state)
        {
            if (_MDataColumn != null)
            {
                MDataTable dt = _MDataColumn._Table;
                if (dt != null && dt.Rows.Count > 0)
                {
                    int index = _MDataColumn.GetIndex(ColumnName);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i].Set(index, value, state);
                    }
                }
            }
            return this;
        }
        /// <summary>
        /// 返回该列的where In 条件 : Name in("aa","bb")
        /// </summary>
        /// <returns></returns>
        public string GetWhereIn()
        {
            if (_MDataColumn != null)
            {
                MDataTable dt = _MDataColumn._Table;
                if (dt != null && dt.Rows.Count > 0)
                {
                    List<string> items = dt.GetColumnItems<string>(ColumnName, BreakOp.NullOrEmpty, true);
                    if (items != null && items.Count > 0)
                    {
                        return SqlCreate.GetWhereIn(this, items, dalType);
                    }
                }
            }
            return string.Empty;
        }
    }
}
