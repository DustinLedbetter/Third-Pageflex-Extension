/***********************************************************************************************************************************
*                                                 GOD First                                                                        *
* Author: Dustin Ledbetter                                                                                                         *
* Release Date: 9-24-2018                                                                                                          *
* Version: 1.0                                                                                                                     *
* Purpose: To create an third extension for the storefront to test how they work                                                   *
************************************************************************************************************************************/

using Pageflex.Interfaces.Storefront;
using System;
using System.Collections.Generic;

namespace MyThirdExtension
{

    public class ExtensionThree : StorefrontExtension
    {


        #region This is used to help shorten code throughout the program

        private ISINI GetSf()
        {
            return Storefront;
        }

        #endregion

        
        #region Extension Name Overides
        // At a minimum your extension must override the DisplayName and UniqueName properties.


        // The UniqueName is used to associate a module with any data that it provides to Storefront.
        public override string UniqueName
        {
            get
            {
                return "ExtensionThree.GetValue.website.com";
            }
        }

        // The DisplayName will be shown on the Extensions and Site Options pages of the Administrator site as the name of your module.
        public override string DisplayName
        {
            get
            {
                return "Extension Three";
            }
        }
        #endregion


        #region This section is used to determine if we are in the "shipping" module on the storefront or not

        public override bool IsModuleType(string x)
        {

            // If we are in the shipping module return true to begin processes for this module
            if (x == "Shipping")
                return true;
            else
                return false;

        }

        #endregion


        #region This section is used to add a payment option to the shipping section

        // EnumeratePickLists
        public override int EnumeratePickLists(out string[] lists)
        {

            // Set the list for return with the Pick List name “Available Shipping Methods”
            lists = new string[1];
            lists[0] = "Available Shipping Methods";

            // Return eSuccess to indicate that the pick list names have been set.
            return eSuccess;

        }


        // MakeItem  - This section is for a method used to make an item when information is passed to it
        private PickListItem MakeItem(string n, string v, bool sel)
        {
            PickListItem pit = new PickListItem();
            pit.SetName(n);
            pit.Value = v;
            pit.IsSelected = sel;
            return pit;
        }


        // GetPickListData  - Gets the pick list items for the given list: "Available Payment Methods".
        public override int GetPickListData(string listname, string fieldType, string intId, bool displayOnly, bool forUser, out PickListItem[] items)
        {

            // Set starting values
            int ReturnValue = 0;
            items = null;
            try
            {

                // Create a list to hold items
                List<PickListItem> picklist = new List<PickListItem>();
                switch (listname)
                {
                    // If the listname "Available Shipping Methods" exists then add an item to it
                    case "Available Shipping Methods":
                        picklist.Add(MakeItem("Courier Service", "CourierServiceCC", true));
                        items = picklist.ToArray();
                        ReturnValue = eSuccess;
                        break;
                    default:
                        ReturnValue = eFailure; break;
                }
            }
            catch (Exception ex)
            {
                // If the list does not exist then log error and return failure
                Storefront.LogMessage("Error getting PickList for " + listname + ", Message " + ex.Message + ", " + ex.StackTrace, null, null, 1, true);
                ReturnValue = eFailure;
            }

            // Return success or failure back to storefront
            return ReturnValue;
        }

        #endregion


        #region This section is used to create the text fields for users to enter data from the extension page and then save it for use in the extension

        // This segment of code is used when a user visits the extension page and clicks on the extension we have created. They will see these lines that they can enter info on.
        // This code ensures they enter data on all of them and saves them.
        private string BuildConfigurationHTML(bool validateAndSave, string handling_charge, string shipping_charge, out bool isValid)
        {
            isValid = true;
            string handling_chargeErrorHTML = "", shipping_chargeErrorHTML = "";
            if (validateAndSave)
            {

                // Checks the handling_charge
                if (string.IsNullOrEmpty(handling_charge))
                {
                    isValid = false;
                    handling_chargeErrorHTML = "<font color=\"red\">You must enter the Handling Charge.</font>";
                }
                else
                {
                    GetSf().SetValue(FieldType.ModuleField, "handling_charge", UniqueName, handling_charge);
                }

                // Checks the shipping_charge
                if (string.IsNullOrEmpty(shipping_charge))
                {
                    isValid = false;
                    shipping_chargeErrorHTML = "<font color=\"red\">You must enter the Shipping Charge.</font>";
                }
                else
                {
                    GetSf().SetValue(FieldType.ModuleField, "shipping_charge", UniqueName, shipping_charge);
                }
               
            }

            // Used to display the information to the user viewing the extension page 
            return "<br><br><b>Configuration</b><br>" +
                "Handling Charge: <input type='text' style=\"min-width: 400px;\" name='shipping_charge' value='" + handling_charge + "'>" + handling_chargeErrorHTML + "<br/>" +
                "Shipping Charge: <input type='text' style=\"min-width: 400px;\" name='handling_charge' value='" + shipping_charge + "'>" + shipping_chargeErrorHTML + "<br/>";

        }


        // this segment of code Gets the information saved on the storefront to be used in the extension
        public override int GetConfigurationHtml(KeyValuePair[] parameters, out string HTML_configString)
        {
            HTML_configString = null;
            try
            {
                string handling_charge = "", shipping_charge = "";
                bool isValid;
                // If parameters is null we want to set the items with values from the BuildConfigurationHTML
                if (parameters == null)
                {
                    handling_charge = GetSf().GetValue(FieldType.ModuleField, "handling_charge", UniqueName);
                    shipping_charge = GetSf().GetValue(FieldType.ModuleField, "shipping_charge", UniqueName);
                 
                    HTML_configString = BuildConfigurationHTML(false, handling_charge, shipping_charge, out isValid);
                }
                else
                {
                    foreach (var p in parameters)
                    {
                        if (p.Name.Equals("handling_charge"))
                        {
                            handling_charge = p.Value;
                        }
                        if (p.Name.Equals("shipping_charge"))
                        {
                            shipping_charge = p.Value;
                        }
                    }
                    var html = BuildConfigurationHTML(true, handling_charge, shipping_charge, out isValid);
                    if (!isValid)
                        HTML_configString = html;
                }
            }
            catch (Exception ex)
            {
                Storefront.LogMessage("GetConfigurationHTML() Error: " + ex.ToString(), null, null, 3, true);
            }
            return eSuccess;
        }

        #endregion


        #region This section shows two methods to change data in the shipping section based on the data entered into the extension page

        // This method sets the handling fee based on the amount given through the extension page
        public override int CalculateHandlingCharge(string orderId, out double handlingCharge, out string isoCurrencyCode)
        {

            // Set the handling charge to the value that was given on the extension page 
            handlingCharge = double.Parse(Storefront.GetValue(FieldType.ModuleField, "handling_charge", UniqueName));
            isoCurrencyCode = "USD";

            // Send the new data back to the storefront for the handling charge
            return eSuccess;
        }


        // This method sets the shipping fee based on the amount given through the extension page
        public override int CalculateShippingCharge2(string orderID, string shipmentID, out double shippingCharge, out string isoCurrencyCode)
        {

            // Set the shipping charge to the value that was given on the extension page 
            shippingCharge = double.Parse(Storefront.GetValue(FieldType.ModuleField, "shipping_charge", UniqueName));
            isoCurrencyCode = "USD";

            // Send the new data back to the storefront for the shipping charge
            return eSuccess;
        }

        #endregion


        //end of the class: ExtensionThree
    }
    //end of the file
}
