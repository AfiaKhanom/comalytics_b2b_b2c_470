  
# Overview  
This document outlines the complete deployment process for the Macsteel NopCommerce 
4.2 application, including Git branching, cherry -picking, deployment to various 
environments (Test, Staging, Live), and post -deployment steps.  
  
# Feature Branch Workflow  
• Create feature branch from macsteel_live/dev , must use trello/jira card number  for 
branch. Every commit must contain the number for easy tracking and filtering.  
• When done, cherry -pick commits from feature branch to Test (based on branch 
number).  
• Cherry -pick from feature branch to Staging (include Dan’s commits  and check CSS 
version if there).  

• Stage to Live: Prefer cherry -pick  over merge to avoid bringing unwanted tasks to Live. 
If Staging = Live and no other pending tasks, merging is allowed.  

# Deployment Prerequisites  
Before deploying to Test / Staging / Live , make a checklist, like:  
• Add Resource strings - done  
• Add Stored Procedures  -  done  
• SQL Jobs prepared  - done  
• Plugin backup - done  
• Plugins replaced (2 plugins)  -  
• App pool restart, site restart -  
• Settings saved in Admin area  -  

# Deployment Steps  
General Rules  
• Always take a backup of the existing plugin . 
• Do not replace — remove old files and paste new ones.  
• After updating .dll files: Recycle Application Pool and Restart site.  
• If you update only view files, still restart the site (required).  
 
 
# Automation & Jenkins (If Need)
After deploying on stage/live, r un automation from Jenkins on Server 184:  
On 184 Go to  the browser : build run for test/live 

# Rollback Procedure  
• Restore the site from backup folder.  
• Restart App Pool and site.  
• Notify the team with incident details. 

# Backup Instructions  
Database:  
• Take a DB backup from SQL Server.  
• Compress it into .zip.  
• If possible remove logs and shrink the log table to reduce size.  
• Upload it to a shared drive from the server.  
• Download to your PC for local storage.  
• Better if we maintain a common folder in oneDrive.

# Additional Notes  
• There is no merge request process in this workflow.  

• All environment transitions are handled through cherry -picking.  
• Always double -check which commits are being brought over to avoid unwanted 
changes.  

 