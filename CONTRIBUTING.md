# Labels

Most labels can be applied to either issues or pull requests. For pull requests that simply address an existing issue,
labels which apply to the topic but are not specific to the pull request are applied to the issue. In the descriptions
below, the word "issue" is used to refer to either an issue or a pull request.

## Impact labels

Impact labels describe the users and/or applications which are affected by an issue.

* **binary**: The issue may affect the ability of the library to load at runtime. Binary breaking changes prevent
  users from using a new release of the assembly as a "drop-in" replacement for the old version.
* **source**: The issue may affect compilation of code referencing the library. For example:
  * Changes to the values of default parameters.
* **runtime**: The issue may affect runtime behavior, especially for cases of reflection and use of the `dynamic`
  type.
* **bug**: The issue describes a bug in the current implementation.
* **enhancement**: The issue describes functionality (code or documentation) which would be beneficial to the project
  in some manner but is not yet implemented.
* **question**: The issue is a question about the project. Questions often lead to the creation of a new issue
  categorized as one of the above (e.g. as an **enhancement** to improve documentation surrounding the topic that led
  to the question).

## Progress labels

Progress labels describe the current status of an issue.

* **up for grabs**: The issue is not currently assigned to anyone or being worked on, and is not designated for review
  by a subject matter expert. Simply add a comment to the issue if you'd like to work on it!
* **in progress**: The issue is currently being worked on.
* **help wanted**: The issue requires further discussion, analysis, or review prior to moving forward. This label is
  applied to pull requests during the code review process; all contributors are especially welcome to look at these.
* **pull request** (issues only): A pull request to address the issue has been created, but not merged. See the pull
  request for more information about its status.
* **do not merge** (pull requests only): The pull request contains issues which must be addressed before it can be
  merged. Reasons for this label include, but are not limited to cases where further discussion is required, a problem
  was identified by code review, or the code is in-progress but not yet complete.
* **blocked**: Progress on the issue is currently blocked. This label may be the result of internal causes, such as a
  requirement that another issue be addressed first, or by external causes, such as waiting on a new release of a
  dependency.

## Resolution labels

Resolution labels describe the outcome of an issue.

* **fixed**: The issue has been addressed. This is only applied to pull requests which are merged and contain changes
  which were not previously reported as an issue.
* **duplicate**: The issue is a duplicate of a previously reported issue. The comments will contain a link to the
  original issue.
* **wontfix**: The code will not change in response to the issue. This is only applied to pull requests which are
  *not* merged and contain changes which were not previously reported as an issue.
* **invalid**: Applied to issues or pull requests that can't (or shouldn't) be evaluated for some reason. This label may
  be the result of any of the following issues (not exclusive):
  * The issue could not be reproduced.
  * The issue was the result of a misunderstanding about the project that is unlikely to affect other users or is
    determined to already be clearly documented.
  * The issue was erroneously submitted, e.g. to the wrong project.
