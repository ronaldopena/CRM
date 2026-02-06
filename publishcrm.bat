cd Poliview.crm
call publish.bat %1 
cd ..

cd Poliview.crm.api
call publish.bat %1 
cd ..

cd Poliview.crm.espacocliente
call publish.bat %1
cd ..

cd Poliview.crm.instalador
call publish.bat %1
cd ..

cd Poliview.crm.mobuss.api
call publish.bat %1
cd ..

cd Poliview.crm.mobuss.integracao
call publish.bat %1
cd ..

cd Poliview.crm.integracao
call publish.bat %1
cd ..

cd Poliview.crm.service.email
call publish.bat %1
cd ..

cd Poliview.crm.sla
call publish.bat %1
cd ..

cd Poliview.crm.monitor.service
call publish.bat %1
cd ..










