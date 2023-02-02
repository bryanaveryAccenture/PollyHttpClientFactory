# PollyHttpClientFactory
FAULT TOLERANCE USING POLLY

#Description - What this project is? purpose of this project
This project is console application based on .Net Core 3.1 framework.
Main purpose of this project is to test fault tolerance sceanrios and confirm those are working as expected with results/logs.

#How to setup and configure this project
1.Fault Tolerance policies used in this application are:
	a)bulkheadisolation - Keeping our application stable when under heavy load.
	The primary purpose of this policy is to prevent application overloading. Its secondary use case is resource allocation.
	By monitoring your 'Bulkhead Isolation' policies, you can determine the number of ongoing parallel requests, as well as the number of requests waiting in the queues. 
	When you reach some limit, you can trigger horizontal scaling and spin up another instance of your application.
	b)Wait and Retry - Our application targets to hit test / mock API. If calling an API throws some error, the retry policy should wait for 5 seconds and call api
	again. Likewise, it should retry 3 times with interval of 1,5,10 seconds.
	c)Circuit Breaker - 
		i)Open circuit - Flow/Circuit is breaked in between for some time
		ii)Closed circuit - Flow/Circuit is running and can send the requests
	-Circuit will break if 2 exceptions/results that are handled by circuit breaker policy are encountered consecutively.
	-Exceptions handled by CB policy are :
	i)Network failures
	ii)HTTP 5XX status codes
	iii)HTTP 408 status code
	-On break, it will log the message in console. Connection stay broken for given period of time, here 5 seconds(durationOfBreak).
	-If first action after (durationOfBreak) time results in exception, circuit will break again for (durationOfBreak) time, if no exception then circuit connection will reset.
2.AppSettings.json:
This has some settings which is configurable while running/testing fault tolerance as below -
Section Name : PollySettings
properties :
1.circuitBreakerOnBreakDuration : circuit breaker policy break duration in seconds
2.maxParallelization : Number of actions/requests executing concurrently / parallely
3.maxQueuingActions :Maximum number of actions/requests queuing , waiting for execution slot

Section Name : RemoteServer
properties :
1.baseAddress : Target API to hit. Once Mock API is in place, this property need to be set with Mock API.
This way will make sure fault tolerance is working as expected.

#How to deploy this for testing or where to deploy this for testing?
Different options are available for publishing.
1.Azure
2.IIS
3.Folder (Standalone)
4.FTP server
5.Docker container

Elaborating Standalone deployment mode below:
1.To enable source code modification for configuration settings change - can use visual studio code
https://code.visualstudio.com/download
2.Install VS Code and open editor
3.Go to Terminal -> new terminal 
	i)type dotnet --help [It provides different commands - Check SDK commands from this]
4.In VisualStudio Code, open PollyClientFactory application. 
5.Modify configuration settings (as per testing scenarios)
6.Run / type terminal command : dotnet build [<PROJECT | SOLUTION NAME>...]
This will build the solution.
7.Run / type command : dotnet publish --configuration Release
This will publish the code at [Project location]\bin\Release\[.net version]\publish
8.Go to location mentioned in #7.
9.Run the exe
10.Check if expected output is displayed.


#Acceptance Criteria [Test Scenarios and configuration]
Given a URL API endpoint the application must retry three times increasing the time between each retry by 1, 5 & 10 seconds
If after 5 times retrying the circuit breaker my be enabled for 30 seconds before the half open - trial period is attempted, if it fails when wait again for 30 seconds until a successful connection has been performed
When the circuit breaker is open then ensure that Failing no traffic returns a 500 is provided.
Ensure that no more than 20 transaction can hit the end point in any one time.
Ensure that 200 transactions can be processed at one time, this should active the bulkhead isolation and queueing process.
No data should be lost.