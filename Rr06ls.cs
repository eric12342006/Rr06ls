using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using FlexSystem.V11;
using FlexSystem.V11.Application.BaseClass;
using FlexSystem.V11.Environment;
using FlexSystem.V11.VirtualClient;
using FlexSystem.V11.VirtualReport;
using FlexSystem.V11.Utils;
using FlexSystem.V11.MultiRange;

using FlexSystem.V11.DomainObject.RentalDObj;
using FlexSystem.V11.IO.Rental;

namespace FlexSystem.V11.Application
{
    public class Rr06ls : ReportBase
    {
        #region Screen Naming Struct
        protected struct ScreenName
        {
            public const string ID = "Rr06ls";

            public struct Grid
            {
                public const string ID = "Grid_UnitTypeSort";

                //public struct Field
                //{
                //    public const string ID = "G_Unit";
                //}
            }
            public struct ReportFields
            {
                public const string ChargeCodeStart = "ChargeCodeStart";
                public const string ChargeCodeEnd = "ChargeCodeEnd";
                public const string BldingStart = "BldingIDStart";
                public const string BldingEnd = "BldingIDEnd";
                public const string AsAtDate = "AsAtDate";
            }

            
        }

        //protected struct ReportFields
        //{
        //    public const string BuildingCodeRange = "BuildingCodeRange";
        //    public const string DocType = "DocType";
        //    public const string MgtCodeRange = "MgtCodeRange";

        //    public const string CustomerName = "CustomerName";
        //}

        #endregion

        protected BuildingDObj buildingDObj;
        protected BuildingDObj.BuildingObj.BuildingHeaderObj buildingHdrObj;

        protected RentalUnitDObj unitDObj;
        protected RentalUnitDObj.RentalUnitObj.UnitHeaderObj unitHdrObj;

        protected ContractDObj contractDObj;
        protected ContractDObj.ContractObj.ContractHdrObj contractHdrObj;
        protected ContractDObj.ContractObj.ContractDetailsObj contractDetailsObj;

        protected UnitTypeDObj unitTypeDObj;
        protected UnitTypeDObj.UnitTypeObj unitTypeObj;

        protected Selection BuildingCodeRange;
        protected SelectionEditorField BuildingCodeField;
        protected SelectionEditor BuildingCodeEditor;

        protected Selection MgtCodeRange;
        protected SelectionEditorField MgtCodeField;
        protected SelectionEditor MgtCodeEditor;

        protected FormGridObj unitTypeSortGrid;

        protected DateTime asAtDate;


        protected List<string> TempList = new List<string>();
        protected List<string> UnitTypeArrayList = new List<string>();

        protected ArrayList Duck = new ArrayList();

        protected Dictionary<string, ArrayList> PrePrintedData;

        public Rr06ls(AppUnitStartingInfo startingInfo, AccessRight accessRight, IAppUnit appUnit)
            : base(startingInfo, accessRight, appUnit)
        {
            this.SDFName = this.GetType().Name;
            this.RPTName = "Rr06ls";
        }

        #region Initialize
        protected override void InitDataClass()
        {
            this.buildingDObj = (BuildingDObj)FxActivator.New(typeof(BuildingDObj));
            this.buildingHdrObj = this.buildingDObj.BuildingObject.BuildingHeaderObject;

            this.unitDObj = (RentalUnitDObj)FxActivator.New(typeof(RentalUnitDObj));
            this.unitHdrObj = this.unitDObj.RentalUnitObject.UnitHeaderObject;

            this.contractDObj = (ContractDObj)FxActivator.New(typeof(ContractDObj));
            this.contractHdrObj = this.contractDObj.ContractObject.ContractHdrObject;
            this.contractDetailsObj = this.contractHdrObj.ContractDetailsObject;

            this.unitTypeDObj = new UnitTypeDObj();
            this.unitTypeObj = this.unitTypeDObj.UnitTypeObject;

            this.unitTypeSortGrid = new FormGridObj(this.virtualClient, ScreenName.Grid.ID, this.unitTypeObj);
            this.PrePrintedData = new Dictionary<string, ArrayList>();
        }

        protected override void InitSelectionClass()
        {
            this.BuildingCodeRange = new Selection(string.Empty, DataType.Character);
            this.BuildingCodeField = new SelectionEditorField("BldingIDStart", "BldingIDEnd", string.Empty, "IsBldingIDMultiSelection");
            this.BuildingCodeEditor = new SelectionEditor(this.BuildingCodeRange, this, this.BuildingCodeField);
            this.AddSelectionEditor(this.BuildingCodeEditor);

            this.MgtCodeRange = new Selection(string.Empty, DataType.Character);
            this.MgtCodeField = new SelectionEditorField("ChargeCodeStart", "ChargeCodeEnd", string.Empty, "IsChargeCodeMultiSelection");
            this.MgtCodeEditor = new SelectionEditor(this.MgtCodeRange, this, this.MgtCodeField);
            this.AddSelectionEditor(this.MgtCodeEditor);
        }
        #endregion

        #region Putoptons, GetOptions, EditOptions
        protected override void EditOptions(EditResponseSet response)
        {
            switch (response.Name)
            {
                case ScreenName.ReportFields.BldingStart:
                case ScreenName.ReportFields.BldingEnd:
                case ScreenName.ReportFields.ChargeCodeStart:
                case ScreenName.ReportFields.ChargeCodeEnd:
                    if (response.FunctionKey == FunctionKey.F9)
                        this.LookUpProgram(response);
                    break;
                case "G_" + UnitType.ControlName.UnitTypeID:

                    if (this.unitTypeObj.CheckKeyExist(response.StringValue) != FxAccess.OK)
                    {
                        this.virtualClient.ShowMessageBox("Unit Type not found");
                        this.unitTypeSortGrid.ClearRow(response.RowIndex);
                    }
                    //if (response.StringValue == this.unitTypeSortGrid.GetRowDataString(this.unitTypeSortGrid.CurrentRowIndex, "G_" + UnitType.ControlName.UnitTypeID))
                    //    break;
                    ////Check UnitType Duplicate
                    //bool TypeExist = false;
                    //for (int rowindex = 1; rowindex <= this.unitTypeSortGrid.DataGridObject.RowCount; rowindex++) 
                    //{
                    //    string TempUnitType = this.unitTypeSortGrid.GetRowDataString(rowindex, "G_" + UnitType.ControlName.UnitTypeID);
                    //    if (response.StringValue == TempUnitType)
                    //    {
                    //        this.virtualClient.ShowMessageBox("Unit Type not found");
                    //        this.unitTypeSortGrid.ClearRow(response.RowIndex);
                    //        TypeExist = true;
                    //    }
                    //}
                    //if(!TypeExist)
                    this.unitTypeSortGrid.SetRowData(response.RowIndex, "G_" + UnitType.ControlName.UnitTypeID, response.StringValue);
                    break;
            }
            base.EditOptions(response);
        }

        protected override void GetOptions()
        {
            for(int rowindex = 1; rowindex <= this.unitTypeSortGrid.DataGridObject.RowCount;rowindex++)
            {
                if(this.unitTypeSortGrid.DataGridObject.IsCurrentRecordEmpty())
                    continue;
                string TempUnitType = this.unitTypeSortGrid.GetRowDataString(rowindex, "G_" + UnitType.ControlName.UnitTypeID);
                if(TempUnitType != string.Empty)
                    TempList.Add(TempUnitType);
            }

            UnitTypeArrayList = TempList.Distinct().ToList();


            if (this.virtualClient.IsControlExist(ScreenName.ReportFields.AsAtDate))
                this.asAtDate = this.virtualClient.GetDataDateTime(ScreenName.ReportFields.AsAtDate);
        
            base.GetOptions();
        }

        protected override void PutOptions()
        {
            base.PutOptions();
        }

        #region ListWin
        protected virtual void LookUpProgram(EditResponseSet response)
        {
            switch (response.Name)
            {
                case ScreenName.ReportFields.ChargeCodeStart:
                case ScreenName.ReportFields.ChargeCodeEnd:
                    //if (response.FunctionKey != FunctionKey.F9)
                    //    return;
                    ////clear
                    this.unitEnv.KeyValue = string.Empty;
                    this.CallProgram("rw21rc");
                    if (this.unitEnv.FunctionKey != FunctionKey.Esc && this.unitEnv.KeyValue != string.Empty)
                    {
                        response.StringValue = this.unitEnv.KeyValue;
                        this.virtualClient.PutData(response.Name, response.StringValue);
                    }
                    break;
                case ScreenName.ReportFields.BldingStart:
                case ScreenName.ReportFields.BldingEnd:
                    if (response.FunctionKey != FunctionKey.F9)
                        return;
                    //clear
                    this.unitEnv.KeyValue = string.Empty;
                    this.CallProgram("Rw07bu");
                    if (this.unitEnv.FunctionKey != FunctionKey.Esc && this.unitEnv.KeyValue != string.Empty)
                    {
                        response.StringValue = this.unitEnv.KeyValue;
                        this.virtualClient.PutData(response.Name, response.StringValue);
                    }
                    break;
            }
        }
        #endregion

        #endregion Putoptons, GetOptions, EditOptions

        #region Load & Save User Settings
        protected override void LoadUserSettings()
        {
            //if (this.userSettings.ContainsKey("BuildingCode"))
            //    this.BuildingCodeRange.SelectionString = this.userSettings["BuildingCode"].ToString();

            //if (this.userSettings.ContainsKey("MgtCode"))
            //    this.MgtCodeRange.SelectionString = this.userSettings["MgtCode"].ToString();

            //#region Grid
            //if (this.userSettings.ContainsKey("UnitSortGrid"))
            //{
            //    string unitTypeSortString = this.userSettings["UnitTypeSortGrid"].ToString();
            //    var unitTypeSortList = unitTypeSortString.Split(new[] { '~' }, StringSplitOptions.RemoveEmptyEntries);

            //    this.unitTypeSortGrid.ClearAllRows();

            //    int rowIndex = 1;
            //    foreach(string curUnitType in unitTypeSortList)
            //    {
            //        this.unitTypeSortGrid.SetRowData(rowIndex, "G_" + UnitType.ControlName.UnitTypeID, curUnitType);
            //        rowIndex++;
            //    }
            //    unitTypeSortGrid.DisplayAllRows();
            //}
            //#endregion Grid
        }

        protected override void SaveUserSettings()
        {
            this.userSettings["BuildingCode"] = this.BuildingCodeRange.SelectionString;
            this.userSettings["MgtCode"] = this.MgtCodeRange.SelectionString;

            //#region Grid
            //string unitTypeSortString = string.Empty;
            //for (int rowIndex = 1; rowIndex <= this.unitTypeSortGrid.DataGridObject.RowCount; rowIndex++)
            //{
            //    string curUnitType = this.unitTypeSortGrid.GetRowDataString(rowIndex, "G_" + UnitTypeArrayList.ControlName.UnitTypeID);
                
            //    unitTypeSortString =
            //        curUnitType == string.Empty ? string.Empty
            //        : (unitTypeSortString != string.Empty ? "~" : "") + curUnitType;
            //}
            //this.userSettings["UnitTypeSortGrid"] = unitTypeSortString;
            //#endregion Grid
        }
        #endregion

        #region Print Record
        protected virtual void PrintInitialize()
        {
            this.buildingHdrObj.ClearCriteria();
            this.buildingHdrObj.SetCriteria(Building.FieldName.BldingID, this.BuildingCodeRange);

            this.buildingHdrObj.Initialize();
            this.contractHdrObj.Initialize();

            this.RecordCount = 0;
        }

        protected virtual void PutHeader()
        {
            //this.virtualReport.PutHeader(ReportFields.BuildingCodeRange, this.BuildingCodeRange.SelectionString);
            //this.virtualReport.PutHeader(ReportFields.DocType, this.selDocType == RentalEmailLogDObj.DocType.Invoice ? "Invoice" : "Receipt");
            //this.virtualReport.PutHeader(ReportFields.MgtCodeRange, this.MgtCodeRange.SelectionString);
            this.virtualReport.PutHeader("Building_BldingName", this.buildingHdrObj.Fields.BldingName + " as at " + this.asAtDate.ToString("MM/dd/yyyy"));
            this.virtualReport.PutHeader("Building_Street", this.buildingHdrObj.Fields.Street);
            //this.virtualReport.PutHeader("AsAtDate", this.asAtDate);

        }

        protected virtual void PrintFooter()
        {
            virtualReport.PutFooter("rec-count", this.RecordCount);
            virtualReport.SendFooter();
        }

        protected override void Print()
        {
            this.PrintInitialize();

            while (this.buildingHdrObj.Next() == FxAccess.OK)
            {
                this.PutHeader();
                
                this.PutBodyData();

               // this.PrintFooter();
            }
            this.TempList.Clear();
            this.UnitTypeArrayList.Clear();
        }

        protected virtual void PutBodyData()
        {       
           // bool HaveRecord = false;
            this.unitTypeObj.Initialize();
            while(this.unitTypeObj.Next() == FxAccess.OK)
            {
                bool isPrintUnitType = false;
                this.unitHdrObj.SetCriteria(RentalUnit.FieldName.UnitType, this.unitTypeObj.Fields.UnitTypeID);
                this.unitHdrObj.Initialize();
                this.unitHdrObj.Fields.BldingID = this.buildingHdrObj.Fields.BldingID;
                while(this.unitHdrObj.Next() == FxAccess.OK
                    &&this.unitHdrObj.Fields.BldingID == this.buildingHdrObj.Fields.BldingID)
                {
                  // HaveRecord = true;
                   if (!isPrintUnitType)
                   {
                       this.virtualReport.BodyGroup = '1';
                       this.virtualReport.PutBody(RentalUnit.ControlName.UnitType, this.unitHdrObj.Fields.UnitType);
                       this.virtualReport.SendBody('1');
                       isPrintUnitType = true;
                   }
                //    this.virtualReport.BodyGroup = ' ';
                 //   this.virtualReport.PutBody(RentalUnit.ControlName.UnitID, this.unitHdrObj.Fields.UnitID);
                    this.virtualReport.SendBody();
                    if (this.PutContractData(this.unitHdrObj.GetKeyString(1)))
                        this.PrintFooter();   
                }
                this.unitHdrObj.ClearCriteria();
                this.contractHdrObj.ClearCriteria();
                this.contractDetailsObj.ClearCriteria();
            }

        }

        protected virtual bool PutContractData(string KeyValue)
        {
            this.virtualReport.BodyGroup = ' ';
            this.virtualReport.PutBody(RentalUnit.ControlName.UnitID, this.unitHdrObj.Fields.UnitID);
            bool fuck = false;
            this.contractDetailsObj.Initialize();
            this.contractDetailsObj.Fields.Resource = KeyValue;
            while(contractDetailsObj.Next(2) == FxAccess.OK
                &&this.contractDetailsObj.Fields.Resource == KeyValue)
            {
            this.contractHdrObj.Fields.ContractID = this.contractDetailsObj.Fields.Source;
            this.contractHdrObj.Fields.Version = this.contractDetailsObj.Fields.Version;
            if (this.contractHdrObj.Read() == FxAccess.OK)
            {
                if (!fuck)
                {
                    this.virtualReport.PutBody(Contract.ControlName.CustomerName, this.contractHdrObj.Fields.CustomerName);
                    fuck = true;
                }
            }
            this.virtualReport.PutBody(ContractDetails.ControlName.Resource, this.contractDetailsObj.Fields.Resource);
            this.virtualReport.PutBody(ContractDetails.ControlName.Desc, this.contractDetailsObj.Fields.Desc);
            this.virtualReport.PutBody(ContractDetails.ControlName.Version, this.contractDetailsObj.Fields.Version);
            this.virtualReport.PutBody(ContractDetails.ControlName.Source, this.contractDetailsObj.Fields.Source);   

            this.virtualReport.SendBody(' ');
            }
            return true;
        }
            //else
            //{
            //    for (int ArrayCount = 0; ArrayCount < UnitTypeArrayList.Count; ArrayCount++)
            //    {
            //        this.unitHdrObj.SetCriteria(RentalUnit.FieldName.BldingID, this.buildingHdrObj.Fields.BldingID);
            //        if (UnitTypeArrayList.Count() != 0)
            //            this.unitHdrObj.SetCriteria(RentalUnit.FieldName.UnitType, UnitTypeArrayList[ArrayCount]);
            //        this.unitHdrObj.Initialize();
            //        while (this.unitHdrObj.Next() == FxAccess.OK)
            //        {
            //            this.virtualReport.BodyGroup = '1';
            //            this.virtualReport.PutBody(RentalUnit.ControlName.UnitType, this.unitHdrObj.Fields.UnitType);
            //            this.virtualReport.SendBody('1');

            //            this.virtualReport.BodyGroup = ' ';
            //            this.virtualReport.PutBody(RentalUnit.ControlName.UnitID, this.unitHdrObj.Fields.UnitID);
            //            this.virtualReport.SendBody();
            //            //string tempKey1 = this.unitHdrObj.GetKeyString(1);
            //            //this.contractDetailsObj.SetCriteria(ContractDetails.FieldName.Resource, tempKey1);
            //            //while (contractDetailsObj.Next() == FxAccess.OK) 
            //            //{
            //            //    this.virtualReport.BodyGroup = ' ';

            //            //    this.virtualReport.SendBody();
            //            //}
            //        }
            //        this.unitHdrObj.ClearCriteria();
            //    }
            //    //this.virtualReport.BodyGroup = ' ';

            //    //this.virtualReport.SendBody();
            //}
        
        #endregion

        #region Close Domain Object
        protected override void CustomCloseDomain()
        {
            if (this.contractHdrObj != null && this.contractHdrObj.OpenTableStatus)
                this.contractHdrObj.Close();

            //if (this.contractDetailsObj != null && this.contractDetailsObj.OpenTableStatus)
            //    this.contractDetailsObj.Close();

            if (this.buildingHdrObj != null && this.buildingHdrObj.OpenTableStatus)
                this.buildingHdrObj.Close();

            if (this.unitTypeObj != null && this.unitTypeObj.OpenTableStatus)
                this.unitTypeObj.Close();

            //if (this.invHdrObj != null && this.invHdrObj.OpenTableStatus)
            //    this.invHdrObj.Close();
            //if (this.dnHdrObj != null && this.dnHdrObj.OpenTableStatus)
            //    this.dnHdrObj.Close();
            //if (this.receiptHdrObj != null && this.receiptHdrObj.OpenTableStatus)
            //    this.receiptHdrObj.Close();
        }
        #endregion
    }
}






            //bool HaveRecord = false;
            //if (UnitTypeArrayList.Count == 0)
            //{
            //    this.unitTypeObj.Initialize();
            //    while (this.unitTypeObj.Next() == FxAccess.OK)
            //    {
            //        //this.PutHeader();
            //        bool isPrintUnitType = false;

            //        this.unitHdrObj.SetCriteria(RentalUnit.FieldName.BldingID, this.buildingHdrObj.Fields.BldingID);
            //        this.unitHdrObj.SetCriteria(RentalUnit.FieldName.UnitType, this.unitTypeObj.Fields.UnitTypeID);
            //        this.unitHdrObj.Initialize();
            //        while (this.unitHdrObj.Next() == FxAccess.OK)
            //        {
            //            HaveRecord = true;
            //            if (!isPrintUnitType)
            //            {
            //                this.virtualReport.BodyGroup = '1';
            //                this.virtualReport.PutBody(RentalUnit.ControlName.UnitType, this.unitHdrObj.Fields.UnitType);
            //                this.virtualReport.SendBody('1');
            //                isPrintUnitType = true;
            //            }
            //            this.virtualReport.BodyGroup = ' ';
            //            this.virtualReport.PutBody(RentalUnit.ControlName.UnitID, this.unitHdrObj.Fields.UnitID);
            //            this.virtualReport.SendBody();
            //            string TempResources = this.unitHdrObj.GetKeyString(1);
            //           // this.contractDetailsObj.SetCriteria(ContractDetails.FieldName.Resource, TempResources);
            //            this.contractDetailsObj.Initialize();
            //            this.contractDetailsObj.Fields.Resource = TempResources;
            //            bool asdasdqwe = false;
            //            while (contractDetailsObj.Next(2) == FxAccess.OK
            //                &&this.contractDetailsObj.Fields.Resource == TempResources)
            //            {
            //                //this.contractHdrObj.Fields.ContractID = this.contractDetailsObj.Fields.Source;
            //                //this.contractHdrObj.Fields.Version = this.contractDetailsObj.Fields.Version;
            //                //if (this.contractHdrObj.Read() == FxAccess.OK && !asdasdqwe)
            //                //{
            //                //    this.virtualReport.PutBody(Contract.ControlName.CustomerName, this.contractHdrObj.Fields.CustomerName);
            //                //    asdasdqwe = true;
            //                //}
            //                this.virtualReport.PutBody(ContractDetails.ControlName.ChargeStrDate, this.contractDetailsObj.Fields.ChargeStrDate.ToString("MM/dd/yyyy"));
            //                this.virtualReport.PutBody(ContractDetails.ControlName.ChargeEndDate, this.contractDetailsObj.Fields.ChargeEndDate.ToString("MM/dd/yyyy"));
            //                this.virtualReport.PutBody(ContractDetails.ControlName.Resource, this.contractDetailsObj.Fields.Resource);
            //                this.virtualReport.PutBody(ContractDetails.ControlName.Extension, this.contractDetailsObj.Fields.Extension);
            //                this.virtualReport.PutBody(ContractDetails.ControlName.Version, this.contractDetailsObj.Fields.Version);
            //               // this.virtualReport.SendBody();
            //            }
            //         //   this.virtualReport.SendBody();
            //            this.RecordCount++;
            //        }
            //        this.unitHdrObj.ClearCriteria();
            //        this.contractHdrObj.ClearCriteria();
            //        this.contractDetailsObj.ClearCriteria();
            //    }
            //    if(HaveRecord)
            //        this.PrintFooter();
            //}