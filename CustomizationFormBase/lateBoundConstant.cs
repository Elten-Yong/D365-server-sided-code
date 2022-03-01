// *********************************************************************
// Created by : Latebound Constants Generator 1.2021.12.1 for XrmToolBox
// Author     : Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://org2a7db823.crm5.dynamics.com
// Filename   : C:\Users\User\Documents\lateBoundConstant.cs
// Created    : 2021-12-17 09:53:27
// *********************************************************************

namespace App.Custom
{
    /// <summary>OwnershipType: UserOwned, IntroducedVersion: 5.0.0.0</summary>
    /// <remarks>Business that represents a customer or potential customer. The company that is billed in business transactions.</remarks>
    public static class Account
    {
        public const string EntityName = "account";
        public const string EntityCollectionName = "accounts";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        /// <remarks>Unique identifier of the account.</remarks>
        public const string PrimaryKey = "accountid";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string totalage = "app_totalage";

        #endregion Attributes
    }

    /// <summary>OwnershipType: UserOwned, IntroducedVersion: 1.0</summary>
    public static class audit
    {
        public const string EntityName = "app_audit";
        public const string EntityCollectionName = "app_audits";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        /// <remarks>Unique identifier for entity instances</remarks>
        public const string PrimaryKey = "app_auditid";
        /// <summary>Type: String, RequiredLevel: ApplicationRequired, MaxLength: 100, Format: Text</summary>
        /// <remarks>The name of the custom entity.</remarks>
        public const string PrimaryName = "app_name";
        /// <summary>Type: Memo, RequiredLevel: None, MaxLength: 2000</summary>
        public const string after = "app_after";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: contact</summary>
        public const string auditfrom = "app_auditfrom";
        /// <summary>Type: Memo, RequiredLevel: None, MaxLength: 2000</summary>
        public const string before = "app_before";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string _event = "app_event";
        /// <summary>Type: Memo, RequiredLevel: None, MaxLength: 2000</summary>
        public const string fieldname = "app_fieldname";

        #endregion Attributes
    }

    /// <summary>OwnershipType: UserOwned, IntroducedVersion: 5.0.0.0</summary>
    /// <remarks>Person with whom a business unit has a relationship, such as customer, supplier, and colleague.</remarks>
    public static class Contact
    {
        public const string EntityName = "contact";
        public const string EntityCollectionName = "contacts";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        /// <remarks>Unique identifier of the contact.</remarks>
        public const string PrimaryKey = "contactid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 160, Format: Text</summary>
        /// <remarks>Combines and shows the contact"s first and last names so that the full name can be displayed in views and reports.</remarks>
        public const string PrimaryName = "fullname";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string age = "app_age";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateOnly, DateTimeBehavior: DateOnly</summary>
        /// <remarks>Enter the contact"s birthday for use in customer gift programs or other communications.</remarks>
        public const string Birthday = "birthdate";
        /// <summary>Type: Customer, RequiredLevel: None, Targets: account,contact</summary>
        /// <remarks>Select the parent account or parent contact for the contact to provide a quick link to additional details, such as financial information, activities, and opportunities.</remarks>
        public const string CompanyName = "parentcustomerid";
        /// <summary>Type: String, RequiredLevel: Recommended, MaxLength: 50, Format: Text</summary>
        /// <remarks>Type the contact"s first name to make sure the contact is addressed correctly in sales calls, email, and marketing campaigns.</remarks>
        public const string FirstName = "firstname";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string incrementvalue = "app_incrementvalue";
        /// <summary>Type: String, RequiredLevel: ApplicationRequired, MaxLength: 50, Format: Text</summary>
        /// <remarks>Type the contact"s last name to make sure the contact is addressed correctly in sales calls, email, and marketing campaigns.</remarks>
        public const string LastName = "lastname";

        #endregion Attributes
    }
}


/***** LCG-configuration-BEGIN *****\
<?xml version="1.0" encoding="utf-16"?>
<Settings xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Version>1.2021.12.1</Version>
  <NameSpace>App.Custom</NameSpace>
  <UseCommonFile>true</UseCommonFile>
  <SaveConfigurationInCommonFile>true</SaveConfigurationInCommonFile>
  <FileName>DisplayName</FileName>
  <ConstantName>DisplayName</ConstantName>
  <ConstantCamelCased>false</ConstantCamelCased>
  <DoStripPrefix>false</DoStripPrefix>
  <StripPrefix>_</StripPrefix>
  <XmlProperties>true</XmlProperties>
  <XmlDescription>true</XmlDescription>
  <Regions>true</Regions>
  <RelationShips>true</RelationShips>
  <RelationshipLabels>false</RelationshipLabels>
  <OptionSets>true</OptionSets>
  <GlobalOptionSets>false</GlobalOptionSets>
  <Legend>false</Legend>
  <CommonAttributes>None</CommonAttributes>
  <AttributeSortMode>None</AttributeSortMode>
  <SelectedEntities>
    <SelectedEntity>
      <Name>account</Name>
      <Attributes>
        <string>accountid</string>
        <string>app_totalage</string>
      </Attributes>
      <Relationships />
    </SelectedEntity>
    <SelectedEntity>
      <Name>app_audit</Name>
      <Attributes>
        <string>app_after</string>
        <string>app_auditid</string>
        <string>app_auditfrom</string>
        <string>app_before</string>
        <string>app_event</string>
        <string>app_fieldname</string>
        <string>app_name</string>
      </Attributes>
      <Relationships />
    </SelectedEntity>
    <SelectedEntity>
      <Name>contact</Name>
      <Attributes>
        <string>app_age</string>
        <string>birthdate</string>
        <string>parentcustomerid</string>
        <string>contactid</string>
        <string>firstname</string>
        <string>fullname</string>
        <string>app_incrementvalue</string>
        <string>lastname</string>
      </Attributes>
      <Relationships />
    </SelectedEntity>
  </SelectedEntities>
</Settings>
\***** LCG-configuration-END   *****/
