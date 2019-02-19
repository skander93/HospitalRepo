var app = angular.module('MyApp', []);
app.controller('MyController', ['$scope', '$http', function ($scope, $http) {
    $scope.SelectedFileForUpload = null;
    $scope.UploadFile = function (files) {
        $scope.$apply(function () { //I have used $scope.$apply because I will call this function from File input type control which is not supported 2 way binding  
            $scope.fileName = files[0].name;
            $scope.SelectedFileForUpload = files[0];
        })
    }
    //Parse Excel Data   
    $scope.ParseExcelDataAndSave = function () {
        var file = $scope.SelectedFileForUpload;
        if (file) {
            var reader = new FileReader();
            reader.onload = function (e) {
                var data = e.target.result;
                //XLSX from js-xlsx library , which I will add in page view page  
                var workbook = XLSX.read(data, { type: 'binary' });
                var sheetName = workbook.SheetNames[1];
                var excelData = XLSX.utils.sheet_to_json(workbook.Sheets[sheetName]);
                var sheetName2 = workbook.SheetNames[0];
                const XLSX_READ_FILE_OPTS = {
                    bookDeps: false,
                    bookFiles: false,
                    bookProps: false,
                    bookSheets: false,
                    bookVBA: false,
                    cellDates: true,
                    cellFormula: false,
                    cellHTML: false,
                    cellNF: false,
                    cellStyles: false,
                    cellText: false,
                    dense: false,
                    raw: true,
                    sheetStubs: false,
                    WTF: false,
                };
                var excelData2 = XLSX.utils.sheet_to_json(workbook.Sheets[sheetName2], XLSX_READ_FILE_OPTS)

                $scope.excelData2 = excelData2;
                $scope.excelData = excelData;

                if (excelData2.length > 0 && excelData.length > 0) {
                    //get file        
                    $scope.getFileByName();
                }
                else {
                    $scope.Message = "No data found";
                }
            }
            reader.onerror = function (ex) {
                console.log(ex);
            }

            reader.readAsBinaryString(file);
        }
    }
    // Save file to our database  
    $scope.SaveFile = function (excelFile) {
        $http({
            method: "POST",
            url: "/Home/ImportFile",
            data: excelFile,
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(function (data) {
            if (data.data.status2 == true) {
                $scope.Message3 = " file inserted ";
                var excelData = $scope.excelData;
                $scope.fileId = data.data.file.pk;
                excelData.forEach(function (element) { element.fileId = data.data.file.pk; });
                $scope.SaveDataSurgeons(excelData);
            }
            else {
                $scope.Message = "Failed";
            }
        }, function (error) {
            $scope.Message = "Error";
        })
    }
    // get file  from our database  
    $scope.getFileByName = function () {
        var fileNameToSave = $scope.fileName;
        var fileJson = { name: fileNameToSave };
        $http({
            method: "POST",
            url: "/Home/GetFile",
            data: fileJson,
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(function (data) {
            if (data.data.status == true) {
                $scope.Message = $scope.fileName + ' is already existing';
            }
            else {
                $scope.SaveFile(fileJson);
            }
        }, function (error) {
            $scope.Message = "Error";
        })
    }
    // Save excel data to our database  
    $scope.SaveDataSurgeons = function (excelData) {
        $http({
            method: "POST",
            url: "/Home/ImportDataSurgeons",
            data: JSON.stringify(excelData),
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(function (data) {
            if (data.data.status == true) {
                $scope.Message = excelData.length + " surgeons inserted";
                var excelData2 = $scope.excelData2;

                excelData2.forEach(function (element) {
                    element.fileId = $scope.fileId;
                    myDate = new Date(element.exitDate);
                    myDate.toJSON();
                    element.exitDate = myDate;
                    myDate2 = new Date(element.entryDate);
                    myDate2.toJSON();
                    element.entryDate = myDate2;
                });

                $scope.saveDataSurgeries(excelData2);
            }
            else {
                $scope.Message = "Failed";
            }
        }, function (error) {
            $scope.Message = "Error";
        })
    }
    // Save excel data to our database  
    $scope.saveDataSurgeries = function (excelData2) {
        $http({
            method: "POST",
            url: "/Home/ImportDataSurgeries",
            data: JSON.stringify(excelData2),
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(function (data) {
            if (data.data.status = true) {
                $scope.Message2 = excelData2.length + " surgeries inserted";
            }
            else {
                $scope.Message = "Failed";
            }
        }, function (error) {
            $scope.Message = "Error";
        })
    }
    // get file  from our database  
    //$scope.FilterData = function () {
    //    var dataToSearch = { 
    //        lastName: $scope.lastName,
    //        firstName: $scope.firstName,
    //        entryDate: $scope.entryDate,
    //        exitDate: $scope.exitDate
    //    };
    //    $http({
    //        method: "POST",
    //        url: "/Home/GetFiltredDataResult",
    //        data: { jsonReceiverInCsharpObjecName: JSON.stringify(dataToSearch) }
    //    }).then(function (data) {
    //        if (data) {
    //            $scope.Message = 'Success !';
                
    //                $scope.surgeries = data.data;

    //        }
    //        else {
    //            $scope.Message = 'No data founded';
    //        }
    //    }, function (error) {
    //        $scope.Message = "Error";
    //    })
    //}
}]);