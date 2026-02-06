$caminho="c:/Deploy"
$versao="4.3.1"

REM rd /s /q "c:/Deploy\CRM\4.3.0"


$app="apicrm"


cd Poliview.crm.api
call publish.bat
cd ..

cd Poliview.crm.espacocliente
call publish.bat
cd ..

cd Poliview.crm.instalador
call publish.bat
cd ..

cd Poliview.crm.mobuss.api
call publish.bat
cd ..

cd Poliview.crm.mobuss.integracao
call publish.bat
cd ..

cd Poliview.crm.integracao
call publish.bat
cd ..

cd Poliview.crm.service.email
call publish.bat
cd ..

cd Poliview.crm.sla
call publish.bat
cd ..

cd Poliview.crm.monitor.service
call publish.bat
cd ..










