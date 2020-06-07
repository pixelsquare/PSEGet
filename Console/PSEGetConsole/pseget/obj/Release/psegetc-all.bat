@echo off

psegetc -t https://www.pse.com.ph/resource/dailyquotationreport/file/ -df 06/01/2020 -dt 06/08/2020 -f ami:"C:\Program Files (x86)\AmiBroker\Data"
::psegetc -l -t https://www.pse.com.ph/resource/dailyquotationreport/file/ -f ami:"C:\Program Files (x86)\AmiBroker\Data"
::psegetc -t https://www.pse.com.ph/resource/dailyquotationreport/file/ -df today -f ami:"C:\Program Files (x86)\AmiBroker\Data"