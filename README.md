# WalletApi

This is a test given by a company.

To run this:
1. Run the ./SQLScript/SQLCreateWebApiDatabase.sql. This will create a new database, tables, stored procedures.
2. Change the config for your local database in appsettings.json
3. Run the application.

There are things I want to improve if I have more time.
1. Implement Authentication and Authorization.
2. Implement more definitive names for the routes such as api version and multiple currency.
3. Implement database indexes.
4. Implement integration testing.

*For the unit testing, there are 3 things I considered to do. I have choosen number 2:*
1. Create a faker services but it is not accurate because I need to create a implementation and it will be different because some of my validations are in sql stored procedure.
2. Test the existing services and need to create all prerequisite instance of the services. This will save the test data into the database but we can create a test database for this so it will not interfere with the live or dev data.
3. Test the existing controller but it can be made in integration testing.
