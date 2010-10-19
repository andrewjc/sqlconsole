using System;
using System.Collections.Generic;
using System.Text;

namespace SQLConsole.Data
{
    public enum ABSTRACTORDERTYPE
    {
        None = 0, Ascending = 1, Descending = 2
    }

    public enum ABSTRACTAGGREGATE
    {
        Sum = 0x00001, Avg = 0x00002, Count = 0x00003, DistinctCount = 0x00004, Min = 0x00005, Max = 0x00006, None = 0x00000
    }

    public enum ABSTRACTDATATYPES
    {
        AString = 1, ASmallInteger = 2, ALargeInteger = 3, AFloat = 4, ABool = 5, AData = 6, ADateTime = 7, AForeignTable = 8, AChar = 9
    }

    public enum ABSTRACTFIELDMODIFIERS
    {
        AutoIncrement = 0x5465743, PrimaryKey = 0x25253232, ForeignKey = 0x0503022, IndexKey = 0x052335235, NotNull = 0x5532332, Clustered = 0x5325233
    }

    public enum ABSTRACTQUERYTYPES
    {
        InsertQuery = 0x55322323, SelectQuery = 0x02020332, UpdateQuery = 0x00022323, DeleteQuery = 0x055532, DropQuery = 0x02323353, AlterQuery = 0x8585332
    }

    public enum ABSTRACTSOURCEBINDTYPES
    {
        INNERJOIN = 0x00001, OUTERJOIN = 0x00002, ALIAS = 0x00003
    }

    public enum ABSTRACTMODIFYACTION
    {
        DropColumn = 0x0001, FieldModify = 0x0002, AddColumn = 0x0003
    }
}
