$DataFolderPath = "C:\Users\Dwight Goins\Documents\CloudPoint\SharePointStorageManagerSolution\Libs"
$Port = "80"
$SiteUrl = "http://localhost/"

$CompleteUrl = $SiteUrl
$build_data_folder = $DataFolderPath

[Reflection.Assembly]::LoadWithPartialName("Microsoft.Office.Interop.Excel")
[System.Reflection.Assembly]::Load("Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c") 
echo "Data Folder Name: $build_data_folder"
$documents = [System.IO.Directory]::GetFiles($build_data_folder,"*.xlsx",[System.IO.SearchOption]::AllDirectories)

if($documents.Count -gt 0)
{
    $site = New-Object Microsoft.SharePoint.SPSite($CompleteUrl)
    $web = $site.OpenWeb()
    
    foreach ($document in $documents){
        $documentItem = get-item $document
        $documentItemName = $documentItem.Name.Split('.')[0]
        
        echo "Loading Data Document: $documentItemName."        
        
        $list =  $web.Lists[$documentItemName]
        if($list -ne $null){
			$existingItems = $list.Items;
			$count = $existingItems.Count - 1
			echo "Deleting  existing items. " $existingItems.Count
			for($intIndex = $count; $intIndex -gt -1; $intIndex--)
			{
					
					$existingItems.Delete($intIndex);
			} 
            echo "Adding new items..."
			$excelConnection= New-Object -com "ADODB.Connection"
            $excelFile=$document
            $excelConnection.Open("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$excelFile;Extended Properties=Excel 12.0;")
            $strQuery="Select * from [Sheet1$]"
            $excelRecordSet=$excelConnection.Execute($strQuery)

            do { 
                $item= $list.AddItem();
   
                foreach ($field in $excelRecordSet.Fields){
                $fieldName= $field.Name
                                
                $item[$fieldName] = $excelRecordSet.Fields.Item($fieldName).Value
              
                }
                $item.Update()
               

                $excelRecordSet.MoveNext()
            }
            Until ($excelRecordSet.EOF)

            $excelConnection.Close()
         
            
        }
        else{
             Write-Error -Message " List $documentItemName NOT founded." -ErrorAction SilentlyContinue
        }
        
        echo "Finish uploading Data Document: $documentItemName."
    }
    
    $web.Dispose();
    $site.Dispose();
}


$objConnection= New-Object -com "ADODB.Connection"
$file = ""

