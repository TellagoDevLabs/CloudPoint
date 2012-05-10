<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Upload.aspx.cs" Inherits="SPSM.Common.Media.Layouts.SPSM.Upload" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server"> 

<table border="0" cellspacing="0" cellpadding="0" class="ms-propertysheet" width="100%">
   <colgroup>
      <col style="width: 30%"></col>
      <col style="width: 70%"></col>
   </colgroup>
   <tr>
      <td>
          <SharePoint:InputFormSection ID="InputFormSection1" runat="server"
               Title="Upload Document"
               Description="Browse to the media asset you intend to upload." >
            <template_inputformcontrols>
               <SharePoint:InputFormControl runat="server" LabelText="" >
                  <Template_Control>                   
                     <div class="ms-authoringcontrols">
						 Name:
                         <br />
                         <input id="FileToUpload" class="ms-fileinput" size="35" type="file" runat="server">    
                         <asp:RequiredFieldValidator id="RequiredFieldValidator" runat="server" ErrorMessage="You must specify a value for the required field." ControlToValidate="FileToUpload"></asp:RequiredFieldValidator>
						 <br />
                         <asp:RegularExpressionValidator id="FileExtensionValidator" runat="server" ErrorMessage="Invalid file name." ControlToValidate="FileToUpload"></asp:RegularExpressionValidator>
                         <br />           
                        <h3 class="ms-standardheader ms-inputformheader">Videos can be:</h3>
                        <ul>
                            <li>Up to <%= mediaConfig.MaxVideoSizeInMegaBytes%> MB file size</li>
                            <li>Up to <%= mediaConfig.MaxVideoLengthMinutes %> mins in length</li>
                            <li>Supported Formats: <%= mediaConfig.SupportedVideoFormats %></li>
                        </ul>
                        <h3 class="ms-standardheader ms-inputformheader">Images can be:</h3>
                        <ul>
                            <li>Up to <%= mediaConfig.MaxImageSizeInMegaBytes %> MB file size</li>
                            <li>Supported Formats: <%= mediaConfig.SupportedImageFormats %> </li>
                        </ul>
                        <h3 class="ms-standardheader ms-inputformheader">Audio can be:</h3>
                        <ul>
                            <li>Up to <%= mediaConfig.MaxAudioSizeInMegaBytes%> MB file size</li>
                            <li>Up to <%= mediaConfig.MaxAudioLengthMinutes %>  mins in length</li>
                            <li>Supported Formats: <%= mediaConfig.SupportedAudioFormats %> </li>
                        </ul>                                                          				
                     </div>
                  </Template_Control>
               </SharePoint:InputFormControl>
            </template_inputformcontrols>
          </SharePoint:InputFormSection>
      </td>
   </tr>
   <tr>
      <td>
          <SharePoint:InputFormSection ID="InputFormSection2" runat="server"
               Title="Upload More Files"
               Description="You can optionally upload thumbnail and poster images related to the principal media asset. If you don't, these images will be generated for Images and Video files, and a default image will be used for Audio files." >
            <template_inputformcontrols>
               <SharePoint:InputFormControl runat="server" LabelText="" >
                  <Template_Control>                   
                     <div class="ms-authoringcontrols">
						 Thumbnail Image:
                         <br />
                         <input id="ThumbnailInput" class="ms-fileinput" size="35" type="file" runat="server">    
                         <asp:RegularExpressionValidator id="ThumbnailFileExtensionValidator" runat="server" ErrorMessage="Invalid file name." ControlToValidate="ThumbnailInput"></asp:RegularExpressionValidator>
                         <br />           
                         Poster Image:
                         <br />
                         <input id="PosterInput" class="ms-fileinput" size="35" type="file" runat="server">    
                         <asp:RegularExpressionValidator id="PosterFileExtensionValidator" runat="server" ErrorMessage="Invalid file name." ControlToValidate="PosterInput"></asp:RegularExpressionValidator>
                         <br />           
                                                                                  				
                     </div>
                  </Template_Control>
               </SharePoint:InputFormControl>
            </template_inputformcontrols>
          </SharePoint:InputFormSection>
      </td>
   </tr>
   <tr><td>
    <SharePoint:ButtonSection runat="server" ShowStandardCancelButton="true">
    <template_buttons>
       <asp:PlaceHolder ID="PlaceHolder1" runat="server">               
           <asp:Button id="btnOk" UseSubmitBehavior="false" runat="server" class="ms-ButtonHeightWidth" 
                       Text="OK" OnClick="btnOk_Click" OnClientClick="this.disabled = true; this.value = 'Uploading. Please wait...';" />
       </asp:PlaceHolder>
    </template_buttons>
</SharePoint:ButtonSection>
    </td></tr>
</table>
  
                                
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Upload Media Asset
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Upload Media Asset
</asp:Content>
