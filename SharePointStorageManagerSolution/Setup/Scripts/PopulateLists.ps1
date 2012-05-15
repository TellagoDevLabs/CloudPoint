Param($SiteUrl,$ScriptsFolder,$DataFolderPath)

&$ScriptsFolder\AddSharePointPSSnapin

$CompleteUrl = $SiteUrl
$build_data_folder = $DataFolderPath

[Reflection.Assembly]::LoadWithPartialName("Microsoft.Office.Interop.Excel")
[System.Reflection.Assembly]::Load("Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c") 
[System.Reflection.Assembly]::Load("Microsoft.SharePoint.Publishing, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c") 
echo "Data Folder Name: $build_data_folder"
$documents = [System.IO.Directory]::GetFiles($build_data_folder,"*.xlsx",[System.IO.SearchOption]::AllDirectories)

if($documents.Count -gt 0)
{
    $site = New-Object Microsoft.SharePoint.SPSite($CompleteUrl)
    $web = $site.OpenWeb()
    
    foreach ($document in $documents){
        $documentItem = get-item $document
        $documentItemName = $documentItem.Name.Split('.')[0]
        
        write-host "Loading Data Document:" $documentItemName -foregroundcolor Green
        
		$length = $documentItemName.Length
		$shortName = $documentItemName.substring(3, $length -3)
		
        $list =  $web.Lists[$shortName]

        if($list -ne $null)
		{
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

			$lookupCount = 0
			$lookupMultiCount = 0
			$imageCount = 0
			$otherCount = 0
					
            do 
			{ 
                $item= $list.AddItem();
   
                foreach ($field in $excelRecordSet.Fields)
				{
					$fieldName= $field.Name
					$fieldNameParts = $fieldName.Split("|");
					$fieldType = $fieldNameParts[0]
					$spFieldName = $fieldNameParts[1]
                                
					if($fieldType -eq "Lookup" -or $fieldType -eq "LookupMulti")
					{
						$sourceListName = "TRS-WorldwidePartnerRegions"
					}
					
					$fieldValue = $excelRecordSet.Fields.Item($fieldName).Value
					
					if(![System.DBNull]::Value.Equals($fieldValue))
					{						
						switch ($fieldType)
						{
							"Lookup"
							{
								$sourceList = $web.Lists[$sourceListName]
							
								$query = New-Object Microsoft.SharePoint.SPQuery

								$query.Query = "<Where><Eq><FieldRef Name='Title' /><Value Type='Text'>" + $fieldValue + "</Value></Eq></Where>"
								$sourceListItems = $sourceList.GetItems($query)
								$sourceListItem = $sourceListItems[0]
								
								if(($sourceListItem.Title -eq $null -or $sourceListItem.Title -eq "") -and $fieldValue -ne "")
								{
									write-host "LookupMulti field missmatch! ->>> '" $fieldValue  "' FieldName = " $spFieldName -foregroundcolor red
								}
								else
								{
									$lookupentry = New-Object Microsoft.Sharepoint.SPFieldLookupValue($sourceListItem.ID,$sourceListItem.Title)
									$item[$spFieldName] = $lookupentry

									$lookupCount += 1
								}
							}
							"LookupMulti"
							{
								$fieldValues = $fieldValue.Split(";")
								$sourceList = $web.Lists[$sourceListName]
								
								$multientry = New-Object Microsoft.Sharepoint.SPFieldMultiChoiceValue($null)
								foreach($value in $fieldValues)
								{										
									$query = New-Object Microsoft.SharePoint.SPQuery
									$query.Query = "<Where><Eq><FieldRef Name='Title' /><Value Type='Text'>" + $value + "</Value></Eq></Where>"
									$sourceListItems = $sourceList.GetItems($query)
									$sourceListItem = $sourceListItems[0]
									
									if(($sourceListItem.Title -eq $null -or $sourceListItem.Title -eq "") -and $value -ne "")
									{
										write-host "LookupMulti field missmatch! ->>> '" $value  "' FieldName = " $spFieldName -foregroundcolor red
									}
									else
									{
										$lookupentry = New-Object Microsoft.Sharepoint.SPFieldLookupValue($sourceListItem.ID,$sourceListItem.Title)
										$multientry.Add($lookupentry)
									}
								}
								
								if($multientry.Count -gt 0)
								{
									$item[$spFieldName] = $multientry

									$lookupMultiCount += 1
								}
							}
							"Image"
							{															
								if($fieldValue.ToLower().Contains("style library/"))
								{
									$urlValue = $fieldValue  			
								}
								else
								{
									$urlValue = "/Lists" + $fieldValue  
								}

								$imageField = New-Object Microsoft.SharePoint.Publishing.Fields.ImageFieldValue($null)
								$imageField.ImageUrl = $urlValue
								$item[$spFieldName] = $imageField

								
								$imageCount += 1
							}
							default
							{
								$item[$spFieldName] = $fieldValue

								
								$otherCount += 1
							}
						}
					}
				
					#$item.Update()
                }

		$item.Update();		

		Start-Sleep 10

                $excelRecordSet.MoveNext()
            }
            Until ($excelRecordSet.EOF)

            $excelConnection.Close()
         
            write-host $lookupCount " Lookup fields populated" -foregroundcolor cyan
			write-host $lookupMultiCount " LooupMulti fields populated" -foregroundcolor cyan
			write-host $imageCount " Image fields populated" -foregroundcolor cyan
			write-host $otherCount " Other fields populated" -foregroundcolor cyan
        }
        else{
             Write-Error -Message " List $documentItemName NOT founded." -ErrorAction SilentlyContinue
        }
        
        write-host "Finish uploading Data Document: " $documentItemName -foregroundcolor Cyan
    }
    
    $web.Dispose();
    $site.Dispose();
}


$objConnection= New-Object -com "ADODB.Connection"
$file = ""

