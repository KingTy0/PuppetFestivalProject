# PuppetFestivalProject
## what is the project?
this is an inventory management web app for the chicago international theatre puppet fest, it is all open source as it was made by a collaboration of students from columbia college chicago using .Net 9.0 / Blazor Web App / MudBlazor / C# / HTML + CSS
## names of the contributers on the repository:
* Asha (he/him)
* * Tables + Data Design
  * EF Core Setup
* Luis (he/him)
* * Branch Management
  * CRUD
  * Import Seed Data
* Tyrone (he/him)
* * CRUD
  * EF Core
  * Product pages
  * Launch to Azure
* Logan (she/her)
* * Display Seed Data
  * Sales page

## Role-based access rules

This project uses the following application roles:

| Feature | Admin | SM | FOH | Driver |
| --- | --- | --- | --- | --- |
| View products, stock, locations, and check status | Yes | Yes | Yes | Yes |
| Box / delivery checks | Yes | Yes | Yes | Yes |
| Sales input | Yes | Yes | Yes | No |
| Product create/edit/delete buttons | Yes | Yes | No | No |
| Direct product create/edit/delete page access | Yes | Yes | No | No |
| User management / storage admin | Yes | No | No | No |

The permission hierarchy is `Admin > SM > FOH > Driver`. Driver has the smallest permission set; there are no Driver-only features.
