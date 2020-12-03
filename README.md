# CordisProject
This project is an internel TOPIC project. It is about getting the data from a server, saving it into a database and display it on a dashboard.
The aim is to have a very nice dashboard that pleases the client CORDIS. The dashboard shall be easy to use and customizable.

## Steps
### Setting up mysql
1-Download mysql, make  sure the server is running and listening to port 3306, username=root, password = root
2-connect to my SQL server (example from the command line), and create an empty database "cordisvariabledb"
3- in the main directory, there is a file "cordisvariabledb_empty". It contains the structure of the database.
Load it into the newly constructed database using the command:
mysql -u root -p cordisvariabledb < cordisvariabledb_empty.sql

At this point, cordisvariabledb shall have all the structure described in the design socument. 
example: use cordisvariabledb;
show tables;
-->  you shall have the following tables: partvalue, partvariable ,valuetype

### Building the Solution
Using Visual studio, open the ClientServerApplication.sln \
The solution contains three parts:\
1-MCDBackend : this is the backend application, responsible of connecting the CORDIS server, reading the data and saving it into the database.\
2-MCDClinet is a simple clinet application, that connects to the MCDBackend to be able to receive signals whenever a data has changed.\
This application is only used if we go for a custom UI solution. For now, we choosed to use GRAFANA to display data, so this project is not really used.\
3-MCSServer is a simulation project, it simulates the CORDIS server. This is where we shall implement and extend our simulation scenarios.\

Build all the projects from Visual studio. All of them shall build ok.\

### Downloading and Setting up Garafana
1-Download Grafana https://grafana.com/grafana/download\
2-install it and makes sure it is running\
3- open local browser and use http://127.0.0.1:3000/    : you shall have grafana welcome page\

### Starting the solution on simulation Mode
1- start the MCDServer application(either from the command line either from VS)\
2- start MCDBackend (either from the command line either from VS)\
At this stage, the backend is receiving data and saving them into the database.  This can be seen in the console consoles.\
3- Open Grafana browser, connect to the MYSQL server and add the database cordisvariabledb\
At this point, you shall be able to see the data saved into the database.\
