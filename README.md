# Third-Pageflex-Extension
This extension checks if we are on the shipping page for ordering, adds a payment option to it, sets up and takes data from the extension creation page and uses it to change the shipping and handling charges on the shipping page. 


This extension builds on two and gets more complicated


Methods:
1. DisplayName ()
2. UniqueName ()
3. private ISINI GetSf () (reduces code throughout project)
4. IsModuleType (string x) (determines if at shipping step)
//5,6, and 7 are used to add a payment option to the shipping section
5. EnumeratePickLists ()
6. MakeItem ()
7. GetPickListData ()
//8 and 9 are used to create the text fields for users to enter data from the extension page and then save it for use in the extension
8. BuildConfigurationHTML ()
9. GetConfigurationHtml ()
10. CalculateHandlingCharge ()
11. CalculateShippingCharge2 ()
