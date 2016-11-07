Unit Testing in .NET Core using dotnet test and VS Code 

Based on https://docs.microsoft.com/en-us/dotnet/articles/core/testing/unit-testing-with-dotnet-test

Wrong configuration files in .vscode folder:
  Delete launch.json and tasks.json, wait for message to appear. 
  Agree to create new files (does not always create correct ones, although...)

Adding FluentAssertions seem to be a breeze. 
Although, running in VS Code, it only indicates pass/fail, with no error messages.
Running it with dotnet test displays them all in full beauty (or none, if tests passed)
... hmm, xunit output seem to be the same in VS Code?

Removing .vscode from git:
- update .gitignore
- git rm --cached .
- git push origin master
Done, easy and clean
