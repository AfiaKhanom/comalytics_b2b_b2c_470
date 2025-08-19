# Comalytics 

## Introduction

**Comalytics Ltd.** , Copyrights 2025 Comalytics. 

Note, **This world-class platform provides your B2B buyers with all the conveniences of a B2C buying journey, combined with the efficiency and functionality of B2B.**


## Getting Started

### Tools We Use

- **Visual Studio 2022**: Our primary development environment.
- **NopCommerce**: An open-source e-commerce solution.
- **SQL Server**: For database management.
- **GitLab**: For version control.
### Tools Needed to Run This Project

- **Visual Studio 2022**: Download and install from [Visual Studio](https://visualstudio.microsoft.com/).
- **SQL Server**: Download and install from [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads).
- **.NET 8.0 SDK**: Ensure it's installed from [.NET SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
- **Git**: Download and install from [Git](https://git-scm.com/).

## Gitflow

We have our own Gitflow policy that implements the best practices for versioning. Visit our gitflow process documentation -

[Git Flow Process](VERSION_CONTROL_AND_RELEASE.md).


## PR Review Guideline
follow the [PR Guideline](PR_Review_Guideline.md) to create and Reveiw Pull Requests.

## Coding Style

We adhere to the following coding styles and best practices:

- **C# Coding Conventions**: Follow the [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).
- **NopCommerce Guidelines**: Nop Commerce has its own coding conventions. Visit this [NopCommerce Coding Convensions](https://docs.nopcommerce.com/en/developer/tutorials/coding-standards.html).
- **NopStation Coding Convention**: Nopstation has its own coding structure. it can be found [here](CodingStructure.md) 
## Project Structure

The NopCommerce project is structured into several key areas:

- **Core**: Contains the core libraries and functionalities of the application.
- **Data**: Manages the database context, entities, and data access logic.
- **Services**: Provides the business logic and service layer between the data layer and the web layer.
- **Web**: Handles the presentation layer, including controllers, views, and front-end assets.

## Database Configuration

First, the  database backup needs to be restored from a backup file of existing database.
After that to configure the database, update the `appsettings.json` file in the Web project with your SQL Server connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=your_server_name\\your_SQL_instance_name;Initial Catalog=your_database_name;Integrated Security=True;Persist Security Info=False;Trust Server Certificate=True""
  }
}
```
## Deployment

For deployment, we follow these steps:

1. **Build**: Ensure the project builds successfully in Visual Studio 2022.
2. **Publish**: NopCommerce plugin upload facility, but in case of size exceeding 50mb login to server through RDP and replace selected build.
3. **Database Update**: Ensure the database is updated with any new migrations.
   feature 

follow the [Deplyment Guideline](Deployment_GuideLine.md) to deploye setp by setp.

## B2B-B2C & ERP Documentation:

For b2b-b2c and erp plugin knowledge:
Follow [Documentation 1](https://brainstationo365-my.sharepoint.com/personal/nopbs_brainstation-23_com/_layouts/15/onedrive.aspx?CT=1755162028183&OR=OWA%2DNTB%2DMail&CID=3468fa4f%2Dbe9e%2D4ea0%2D8177%2Df7de74de74c3&e=5%3A05fd532fb1df48efa0f33e4371a82548&sharingv2=true&fromShare=true&at=9&FolderCTID=0x01200048E50B89D2DE024CA989FB883225497B&id=%2Fpersonal%2Fnopbs%5Fbrainstation%2D23%5Fcom%2FDocuments%2Fnopbs%5FnopStation%2FnopStation%5FClients%2FNCL0001%5FJannie%5FComalytics%2FnopStation%20Documentation%2FB2B%2FnopStation%20B2B%20B2C%20and%20Integration%20Documentation%20%281%29%2Epdf&parent=%2Fpersonal%2Fnopbs%5Fbrainstation%2D23%5Fcom%2FDocuments%2Fnopbs%5FnopStation%2FnopStation%5FClients%2FNCL0001%5FJannie%5FComalytics%2FnopStation%20Documentation%2FB2B)
[Documentation 2](https://brainstationo365-my.sharepoint.com/personal/nopbs_brainstation-23_com/_layouts/15/onedrive.aspx?CT=1755162028183&OR=OWA%2DNTB%2DMail&CID=3468fa4f%2Dbe9e%2D4ea0%2D8177%2Df7de74de74c3&e=5%3A05fd532fb1df48efa0f33e4371a82548&sharingv2=true&fromShare=true&at=9&FolderCTID=0x01200048E50B89D2DE024CA989FB883225497B&id=%2Fpersonal%2Fnopbs%5Fbrainstation%2D23%5Fcom%2FDocuments%2Fnopbs%5FnopStation%2FnopStation%5FClients%2FNCL0001%5FJannie%5FComalytics%2FnopStation%20Documentation%2FB2B%2FB2B%20User%20Manual%2Epdf&parent=%2Fpersonal%2Fnopbs%5Fbrainstation%2D23%5Fcom%2FDocuments%2Fnopbs%5FnopStation%2FnopStation%5FClients%2FNCL0001%5FJannie%5FComalytics%2FnopStation%20Documentation%2FB2B)


## QA & Developer Feedback
[QA Feedback](https://brainstationo365-my.sharepoint.com/:x:/r/personal/nopbs_brainstation-23_com/_layouts/15/Doc.aspx?sourcedoc=%7BF348E081-4E54-4A60-A909-924A0D16F745%7D&file=Comalytics%20Testing-v1.xlsx&action=default&mobileredirect=true)

## Server Details and Site credential
[Bitwarden](https://bitwarden.com/)
nopstation_comalytics@outlook.com
%T#,WyS!ATuh7zW
[Outlook](https://outlook.office365.com/)
nopstation_comalytics@outlook.com
Brain@2323