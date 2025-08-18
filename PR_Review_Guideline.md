# PR Review Guideline

## Naming Conventions

- **Branch Name**: branch names should follow the following convension - `feature/FFN-{ticket_no}-{Ticket_title}`
for example if the jira ticket no is 498 and ticket title is **11.5.1 Payment Confirmation- Development** 
the branch name should be
    ```
        feature/FFN-498-11.5.1-Payment-Confirmation-Development
    ```

- **PR Name**: The PR title should be exactly the same as the branch name. from above example it should be 
    ```
        feature/FFN-498-11.5.1-Payment-Confirmation-Development
    ```

- **Commit Message**: Each commit message should follow the following commit message convention-
 `FFN - {ticketNo} - {commit message}`
like in the above example commit messages for branch 
*feature/FFN-498-11.5.1-Payment-Confirmation-Development* should be like -
    ```
    FFN-498-Refactored services and factories
    ``` 
## PR Assignment and Tracking

- **Assign Yourself**: If you are the assignee or the reviewer, make sure to assign yourself to the PR. 
- **Add PR to Review Sheet**: Add the PR link to the [PR sheet](https://tinyurl.com/2be5zsg8).

## PR Checklist

1. **Target Branch**: Ensure that the PR is created to the correct target branch.
2. **PR Template**: Verify that the PR template is used correctly.
3. **Feedback**: Provide feedback precisely. If detailed discussion is required, consider discussing 1-on-1. Give feedback to specific lines you find problems on. Suggest or discuss what can be improved and what needs to be fixed or changed.
4. **Source Branch**: Do not tick the 'Delete source branch' option.
5. **Checklist Maintain**: A reviewer should check all the points added checklist in this [PR template](PR_Template.md) while reviewing the PR and will make sure that each point has been maintained properly.

## Post-Review

- **Update PR Review Sheet**: After completing the review, update the [PR sheet](https://tinyurl.com/2be5zsg8) accordingly. 