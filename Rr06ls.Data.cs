//using System;
//using System.Linq;
//using System.Collections.Generic;
//using System.Text;
//using System.Collections;

//using FlexSystem.V11;
//using FlexSystem.V11.Application.BaseClass;
//using FlexSystem.V11.Environment;
//using FlexSystem.V11.VirtualClient;
//using FlexSystem.V11.VirtualReport;
//using FlexSystem.V11.Utils;
//using FlexSystem.V11.MultiRange;

//using FlexSystem.V11.DomainObject.RentalDObj;
//using FlexSystem.V11.IO.Rental;

//namespace FlexSystem.V11.Application
//{
//    public partial class Rr06ls
//    {
//        #region Print Record
//        protected virtual void PrintInitialize()
//        {
//            this.buildingHdrObj.ClearCriteria();
//            this.buildingHdrObj.SetCriteria(Building.FieldName.BldingID, this.BuildingCodeRange);

//            this.buildingHdrObj.Initialize();
//            this.contractHdrObj.Initialize();

//            this.RecordCount = 0;
//        }

//        protected virtual void PutHeader()
//        {
//            //this.virtualReport.PutHeader(ReportFields.BuildingCodeRange, this.BuildingCodeRange.SelectionString);
//            //this.virtualReport.PutHeader(ReportFields.DocType, this.selDocType == RentalEmailLogDObj.DocType.Invoice ? "Invoice" : "Receipt");
//            //this.virtualReport.PutHeader(ReportFields.MgtCodeRange, this.MgtCodeRange.SelectionString);
//            this.virtualReport.PutHeader("Building_BldingName", this.buildingHdrObj.Fields.BldingName + " as at " + this.asAtDate.ToString("MM/dd/yyyy"));
//            this.virtualReport.PutHeader("Building_Street", this.buildingHdrObj.Fields.Street);
//            //this.virtualReport.PutHeader("AsAtDate", this.asAtDate);

//        }

//        protected virtual void PrintFooter()
//        {
//            virtualReport.PutFooter("rec-count", this.RecordCount);
//            virtualReport.SendFooter();
//        }

//        protected override void Print()
//        {
//            this.PrintInitialize();

//            while (this.buildingHdrObj.Next() == FxAccess.OK)
//            {
//                this.PutHeader();

//                this.PutBodyData();

//                // this.PrintFooter();
//            }
//            this.TempList.Clear();
//            this.UnitTypeArrayList.Clear();
//        }

//        protected virtual void PutBodyData()
//        {
//            // bool HaveRecord = false;
//            this.unitTypeObj.Initialize();
//            while (this.unitTypeObj.Next() == FxAccess.OK)
//            {
//                bool isPrintUnitType = false;
//                this.unitHdrObj.SetCriteria(RentalUnit.FieldName.UnitType, this.unitTypeObj.Fields.UnitTypeID);
//                this.unitHdrObj.Initialize();
//                this.unitHdrObj.Fields.BldingID = this.buildingHdrObj.Fields.BldingID;
//                while (this.unitHdrObj.Next() == FxAccess.OK
//                    && this.unitHdrObj.Fields.BldingID == this.buildingHdrObj.Fields.BldingID)
//                {
//                    // HaveRecord = true;
//                    if (!isPrintUnitType)
//                    {
//                        this.virtualReport.BodyGroup = '1';
//                        this.virtualReport.PutBody(RentalUnit.ControlName.UnitType, this.unitHdrObj.Fields.UnitType);
//                        this.virtualReport.SendBody('1');
//                        isPrintUnitType = true;
//                    }
//                    //    this.virtualReport.BodyGroup = ' ';
//                    //   this.virtualReport.PutBody(RentalUnit.ControlName.UnitID, this.unitHdrObj.Fields.UnitID);
//                    this.virtualReport.SendBody();
//                    if (this.PutContractData(this.unitHdrObj.GetKeyString(1)))
//                        this.PrintFooter();
//                }
//                this.unitHdrObj.ClearCriteria();
//                this.contractHdrObj.ClearCriteria();
//                this.contractDetailsObj.ClearCriteria();
//            }

//        }

//        protected virtual bool PutContractData(string KeyValue)
//        {
//            this.virtualReport.BodyGroup = ' ';
//            this.virtualReport.PutBody(RentalUnit.ControlName.UnitID, this.unitHdrObj.Fields.UnitID);
//            bool fuck = false;
//            this.contractDetailsObj.Initialize();
//            this.contractDetailsObj.Fields.Resource = KeyValue;
//            while (contractDetailsObj.Next(2) == FxAccess.OK
//                && this.contractDetailsObj.Fields.Resource == KeyValue)
//            {
//                this.contractHdrObj.Fields.ContractID = this.contractDetailsObj.Fields.Source;
//                this.contractHdrObj.Fields.Version = this.contractDetailsObj.Fields.Version;
//                if (this.contractHdrObj.Read() == FxAccess.OK)
//                {
//                    if (!fuck)
//                    {
//                        this.virtualReport.PutBody(Contract.ControlName.CustomerName, this.contractHdrObj.Fields.CustomerName);
//                        fuck = true;
//                    }
//                }
//                this.virtualReport.PutBody(ContractDetails.ControlName.Resource, this.contractDetailsObj.Fields.Resource);
//                this.virtualReport.PutBody(ContractDetails.ControlName.Desc, this.contractDetailsObj.Fields.Desc);
//                this.virtualReport.PutBody(ContractDetails.ControlName.Version, this.contractDetailsObj.Fields.Version);
//                this.virtualReport.PutBody(ContractDetails.ControlName.Source, this.contractDetailsObj.Fields.Source);

//                this.virtualReport.SendBody(' ');
//            }
//            return true;
//        }
//        //else
//        //{
//        //    for (int ArrayCount = 0; ArrayCount < UnitTypeArrayList.Count; ArrayCount++)
//        //    {
//        //        this.unitHdrObj.SetCriteria(RentalUnit.FieldName.BldingID, this.buildingHdrObj.Fields.BldingID);
//        //        if (UnitTypeArrayList.Count() != 0)
//        //            this.unitHdrObj.SetCriteria(RentalUnit.FieldName.UnitType, UnitTypeArrayList[ArrayCount]);
//        //        this.unitHdrObj.Initialize();
//        //        while (this.unitHdrObj.Next() == FxAccess.OK)
//        //        {
//        //            this.virtualReport.BodyGroup = '1';
//        //            this.virtualReport.PutBody(RentalUnit.ControlName.UnitType, this.unitHdrObj.Fields.UnitType);
//        //            this.virtualReport.SendBody('1');

//        //            this.virtualReport.BodyGroup = ' ';
//        //            this.virtualReport.PutBody(RentalUnit.ControlName.UnitID, this.unitHdrObj.Fields.UnitID);
//        //            this.virtualReport.SendBody();
//        //            //string tempKey1 = this.unitHdrObj.GetKeyString(1);
//        //            //this.contractDetailsObj.SetCriteria(ContractDetails.FieldName.Resource, tempKey1);
//        //            //while (contractDetailsObj.Next() == FxAccess.OK) 
//        //            //{
//        //            //    this.virtualReport.BodyGroup = ' ';

//        //            //    this.virtualReport.SendBody();
//        //            //}
//        //        }
//        //        this.unitHdrObj.ClearCriteria();
//        //    }
//        //    //this.virtualReport.BodyGroup = ' ';

//        //    //this.virtualReport.SendBody();
//        //}

//        #endregion
//    }
//}
