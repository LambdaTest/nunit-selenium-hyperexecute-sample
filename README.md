# How to run Selenium automation tests on Hypertest (using C# NUnit framework)

Download the concierge binary corresponding to the host operating system. It is recommended to download the binary in the project's Parent Directory.

* Mac: https://downloads.lambdatest.com/concierge/darwin/concierge
* Linux: https://downloads.lambdatest.com/concierge/linux/concierge
* Windows: https://downloads.lambdatest.com/concierge/windows/concierge.exe

[Note - The current project has concierge for macOS. Irrespective of the host OS, the concierge will auto-update whenever there is a new version on the server]

## Running tests in NUnit using the Matrix strategy

Matrix YAML file (nunit_hypertest_matrix_sample.yaml) in the repo contains the following configuration:

```yaml
globalTimeout: 90
testSuiteTimeout: 90
testSuiteStep: 90
```

Global timeout, testSuite timeout, and testSuite timeout are set to 90 minutes.

The target platform is set to macOS

```yaml
 os: [mac]
```

A user-defined key *project* is set to the C-Sharp project location  (i.e. .csproj). Hence, the matrix comprises of *os* and *project* keys, details of which are shown below:

```yaml
matrix:
  os: [mac]
  project: ["NUnitHyperTestDemo/NUnitHyperTestDemo.csproj"]
```

Content under the *pre* directive is the pre-condition that will be run before the tests are executed on Hypertest grid. The "dotnet install" script for macOS & Windows is downloaded and kept in the project root directory. The stable version of the scripts can be downloaded from [Microsoft Official Website](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-install-script).

* [Bash - Linux/macOS](https://dot.net/v1/dotnet-install.sh)
* [PowerShell for Windows](https://dot.net/v1/dotnet-install.ps1)

However, this is an optional step and can be skipped from the *pre* directive. Once downloaded, we install the LTS release using the commands mentioned [here](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-install-script#examples). We set the permissions of C-Sharp project to 777 (i.e. rwx).

```yaml
pre:
   - ./dotnet-install.sh --channel LTS
   - chmod +rwx NUnitHyperTestDemo/NUnitHyperTestDemo.csproj
```

The *testSuites* object contains a list of commands (that can be presented in an array). In the current YAML file, commands to be run for executing the tests are put in an array (with a '-' preceding each item). In the current YAML file, *dotnet test* command is used for executing the tests present in the *$project* key (i.e. "NUnitHyperTestDemo/NUnitHyperTestDemo.csproj")

```yaml
testSuites:
  - dotnet test $project
```

The [user_name and access_key of LambdaTest](https://accounts.lambdatest.com/detail/profile) is appended to the *concierge* command using the *--user* and *--key* command-line options. The CLI option *--config* is used for providing the custom Hypertest YAML file (e.g. nunit_hypertest_matrix_sample.yaml). Run the following command on the terminal to trigger the tests in C# project on the Hypertest grid.

```bash
./concierge --user "${ YOUR_LAMBDATEST_USERNAME()}" --key "${ YOUR_LAMBDATEST_ACCESS_KEY()}" --config nunit_hypertest_matrix_sample.yaml --verbose
```

Visit [Hypertest Automation Dashboard](https://automation.lambdatest.com/hypertest) to check the status of execution

## Running tests in NUnit using Auto-split execution

Matrix YAML file (nunit_hypertest_autosplit_sample.yaml) in the repo contains the following configuration:

```yaml
globalTimeout: 90
testSuiteTimeout: 90
testSuiteStep: 90
```

Global timeout, testSuite timeout, and testSuite timeout are set to 90 minutes.

The *runson* key determines the platform (or operating system) on which the tests would be executed. Here we have set the target OS as macOS.

```yaml
runson: mac
```

Auto-split is set to true in the YAML file.

```yaml
autosplit: true
```

Retry on failure is set to False and the concurrency (i.e. number of parallel sessions) is set to 1. If the test execution fails (at the first shot), further attempts for execution would not be made.

```yaml
retryOnFailure: false
concurrency: 1
```

Content under the *pre* directive is the pre-condition that will be run before the tests are executed on Hypertest grid.
The "dotnet install" script for macOS & Windows is downloaded and kept in the project root directory. The stable version of the scripts are downloaded from [Microsoft Official Website](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-install-script).

* [Bash - Linux/macOS](https://dot.net/v1/dotnet-install.sh)
* [PowerShell for Windows](https://dot.net/v1/dotnet-install.ps1)

However, this is an optional step and can be skipped from the *pre* directive. Once downloaded, we install the LTS release using the commands mentioned [here](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-install-script#examples). We set the permissions of C-Sharp project to 777 (i.e. rwx).

```yaml
pre:
   - ./dotnet-install.sh --channel LTS
   - chmod +rwx NUnitHyperTestDemo/NUnitHyperTestDemo.csproj
```

The *testDiscoverer* contains the command that locates the C# project (i.e. .csproj). The output of the *testDiscoverer* command is passed in the *testRunnerCommand*

```bash
find NUnitHyperTestDemo -type f -name "*.csproj"
```

Running the above command on the terminal gives the following output:

* NUnitHyperTestDemo/NUnitHyperTestDemo.csproj

The [user_name and access_key of LambdaTest](https://accounts.lambdatest.com/detail/profile) is appended to the *concierge* command using the *--user* and *--key* command-line options. The CLI option *--config* is used for providing the custom Hypertest YAML file (e.g. nunit_hypertest_autosplit_sample.yaml). Run the following command on the terminal to trigger the tests in C# project on the Hypertest grid.

```bash
./concierge --user "${ YOUR_LAMBDATEST_USERNAME()}" --key "${ YOUR_LAMBDATEST_ACCESS_KEY()}" --config nunit_hypertest_autosplit_sample.yaml --verbose

Visit [Hypertest Automation Dashboard](https://automation.lambdatest.com/hypertest) to check the status of execution