Imports System.Drawing.Imaging
Imports System.IO
Imports System.Security.Cryptography
Imports System.Security.Cryptography.X509Certificates
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Windows.Forms.VisualStyles
Imports WMPLib

Public Class Form1
    Public encodingASCII As Encoding = System.Text.Encoding.ASCII

    Public PlayingMedia As Boolean = False
    Public BrokenSteel As Boolean = False
    Public CodeStage As Integer = 0
    Public CodeName As String
    Public CodeExtra As String
    Public SysTime As String

    Public Media_IntroRepeat As Boolean = False
    Public Media_NewsIntroRepeat As Boolean = False
    Public Media_NewsRepeat As Boolean = False
    Public Media_SongRepeat As Boolean = False
    Public Media_OutroRepeat As Boolean = False
    Public Media_PSAIntroRepeat As Boolean = False
    Public Media_PSARepeat As Boolean = False
    Public Media_FirstTimePlay As Boolean = True


    Public NewsList() As String
    Public Media_Intros() As String = {"Intro01", "Intro02", "Intro03", "Intro04", "Intro05", "Intro06", "Intro07", "Intro08", "Intro09", "Intro10", "Intro11", "Intro12", "Intro13", "Intro14"}
    Public Media_NewsIntros() As String = {"NewsIntro01", "NewsIntro02", "NewsIntro03", "NewsIntro04", "NewsIntro05", "NewsIntro06", "NewsIntro07", "NewsIntro08", "NewsIntro09", "NewsIntro10"}
    Public Media_Outros() As String = {"Outro01", "Outro02", "Outro03", "Outro04"}
    Public Media_PSAIntros() As String = {"PSAIntro01", "PSAIntro02", "PSAIntro03", "PSAIntro04"}
    Public Media_PSAs() As String = {"PSA01", "PSA02", "PSA03", "PSA04", "PSA05", "PSA06"}

    Public MainQuestCompleted As String
    Public PlayerLevel As Integer
    Public PlayerLevelS As String
    Public PlayerLevelRandom As Boolean = False
    Public PlayerKarma As String
    Public PlayerGender As String
    Public DJ As String
    Public SongURL As String
    Public SongTitle As String
    Public DashwoodP As Integer
    Public DashwoodStage As Integer = 1

    Public SongMin As Integer
    Public SongMax As Integer


    Public WMPState As Integer = 1
    Public MediaFile As String
    Public MediaFile_PlayerIntro As String

    Public SaveString As String
    Public LoadFileName As String = "Configs\Default.txt"



    '
    'Intro <-> Herbert Dashwood
    'News Intro -> Player Intro
    'News
    'Outro <-> PSA Intro <-> PSA
    'Song 1
    'Song 2
    'Song 3

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ckbDebug_CheckedChanged(Nothing, Nothing)

        AxWindowsMediaPlayer1.settings.autoStart = True

        lkbAbout.Text = "v" & My.Application.Info.Version.ToString

        cmbLevel.SelectedIndex = 0

        lbxMediaIntros.Items.AddRange(Media_Intros)
        lbxMediaNewsIntros.Items.AddRange(Media_NewsIntros)
        lbxMediaOutros.Items.AddRange(Media_Outros)
        lbxMediaPSAIntros.Items.AddRange(Media_PSAIntros)
        lbxMediaPSAs.Items.AddRange(Media_PSAs)

        DJ = "ThreeDog"
        For Each foundFile As String In My.Computer.FileSystem.GetFiles("Assets\" & DJ & "\OutroMusic\")
            Dim FileName As String = Path.GetFileName(foundFile)
            lbxMediaOutrosSongs.Items.Add(FileName)
        Next
        For Each foundFile As String In My.Computer.FileSystem.GetFiles("Assets\" & DJ & "\IntroMusic\")
            Dim FileName As String = Path.GetFileName(foundFile)
            lbxMediaIntrosSongs.Items.Add(FileName)
        Next

        ckbOtherMusicExclusive.Checked = False
        ckbOtherMusicExclusive.Enabled = False

        SongMin = txtSongCountPerMin.Text
        SongMax = txtSongCountPerMax.Text

        LoadFile()

        CodeName = "Form Load =========="
        DebugLogging()

    End Sub



    Private Sub CmdGO_Click(sender As Object, e As EventArgs) Handles cmdGO.Click
        PlayingMedia = True
        cmdGO.Enabled = False
        cmdStop.Enabled = True

        SetStats()

        CodeStage += 1
        CodeName = "Go Button"
        DebugLogging()

        grbMainQuests.Enabled = False
        grbSideQuests.Enabled = False
        grbSideQuestSettings.Enabled = False
        gbxPlayerStats.Enabled = False

        If ckbCompactPlayer.Checked = True Then
            grbSideQuestSettings.Visible = False
            grbSideQuests.Visible = False
            grbMainQuests.Visible = False
            gbxPlayerStats.Visible = False
            grbDJ.Visible = False
            grbMusicSelection.Visible = False
            grbSaveLoadButtons.Visible = False
        End If
        MasterPlayLoop()
    End Sub
    Private Sub CmdStop_Click(sender As Object, e As EventArgs) Handles cmdStop.Click
        cmdStop.Enabled = False
        cmdGO.Enabled = True
        PlayingMedia = False

        AxWindowsMediaPlayer1.Ctlcontrols.stop()
        AxWindowsMediaPlayer1.currentPlaylist.clear()

        grbMainQuests.Enabled = True
        grbSideQuests.Enabled = True
        grbSideQuestSettings.Enabled = True
        gbxPlayerStats.Enabled = True

        If ckbCompactPlayer.Checked = True Then
            grbSideQuestSettings.Visible = True
            grbSideQuests.Visible = True
            grbMainQuests.Visible = True
            gbxPlayerStats.Visible = True
            grbDJ.Visible = True
            grbMusicSelection.Visible = True
            grbSaveLoadButtons.Visible = True
        End If

        Media_FirstTimePlay = True

        lbxMediaIntrosPlayed.Items.Clear()
        lbxMediaNewsIntrosPlayed.Items.Clear()
        lbxMediaNewsPlayed.Items.Clear()
        lbxMediaOutrosPlayed.Items.Clear()
        lbxMediaPSAIntrosPlayed.Items.Clear()
        lbxMediaPSAPlayed.Items.Clear()
        lbxMediaFalloutSongsPlayed.Items.Clear()
        lbxMediaFalloutSongs.Items.Clear()

    End Sub

    Private Sub MasterPlayLoop()
        Dim RNGen As New Random
        Dim RandomNum As Integer


        If DJ = "ThreeDog" Then
            CodeStage += 1
            CodeName = "Master Play Loop Entered"
            CodeExtra = "ThreeDog"
            DebugLogging()

            ChooseIntro_1()
            RandomNum = RNGen.Next(0, 100)
            If ckbDashwood.Checked And RandomNum > (100 - DashwoodP) Then
                ChooseDashwood()
                ChooseIntro_2()
            End If
            ChooseNewsIntro()
            ChooseNews()

            If txtSongCountPerMin.Text = txtSongCountPerMax.Text Then
                For X As Integer = 1 To txtSongCountPerMin.Text
                    ChooseSongs()
                Next X
            Else
                RandomNum = RNGen.Next(txtSongCountPerMin.Text, txtSongCountPerMax.Text)
                For X As Integer = 1 To RandomNum
                    ChooseSongs()
                Next X

            End If



            ' ChooseSongs()
            RandomNum = RNGen.Next(100)
            If RandomNum > 65 Then 'Check for PSA before Outro, to determine if a Song-Related Outro is acceptable
                ChooseOutro_1()
                ChoosePSAIntro()
                ChoosePSA()
            End If
            ChooseOutro_2()
            PlaySongs()

            AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\" & DJ & "\" & "Silence.mp3"))

        ElseIf rdbDJ_Margaret.Checked Then

        Else
            For X As Integer = 1 To txtSongCountPerMin.Text
                ChooseSongs()
            Next X
            PlaySongs()
            AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\" & DJ & "\" & "Silence.mp3"))
        End If
        AxWindowsMediaPlayer1.Ctlcontrols.play()

    End Sub

    Private Sub SetStats()
        If rdbKarmaGood.Checked Then
            PlayerKarma = "G"
        ElseIf rdbKarmaNeutral.Checked Then
            PlayerKarma = "N"
        ElseIf rdbKarmaEvil.Checked Then
            PlayerKarma = "E"
        Else

        End If
        If rdbGenderMale.Checked Then
            PlayerGender = "M"
        Else
            PlayerGender = "F"
        End If

        PlayerLevel = cmbLevel.SelectedItem
        If PlayerLevel = 0 Then
            PlayerLevelRandom = True
        End If

        If PlayerLevel < 10 Then
            PlayerLevelS = "0" & PlayerLevel
        Else
            PlayerLevelS = PlayerLevel
        End If

        If ckbOtherMusic.Checked Then
            For Each foundFile As String In My.Computer.FileSystem.GetFiles("Assets\Music\")
                Dim FileName As String = Path.GetFileName(foundFile)
                lbxMediaFalloutSongs.Items.Add(FileName)
            Next
        End If

        If ckbOtherMusicExclusive.Checked = True Then

        Else
            If rdbMusicModeMono.Checked Then
                For Each foundFile As String In My.Computer.FileSystem.GetFiles("Assets\Fallout Songs_Mono\")
                    Dim FileName As String = Path.GetFileName(foundFile)
                    lbxMediaFalloutSongs.Items.Add(FileName)
                Next
            ElseIf rdbMusicModeClear.Checked Then
                For Each foundFile As String In My.Computer.FileSystem.GetFiles("Assets\Fallout Songs\")
                    Dim FileName As String = Path.GetFileName(foundFile)
                    lbxMediaFalloutSongs.Items.Add(FileName)
                Next
            Else
                For Each foundFile As String In My.Computer.FileSystem.GetFiles("Assets\Fallout Songs\")
                    Dim FileName As String = Path.GetFileName(foundFile)
                    lbxMediaFalloutSongs.Items.Add(FileName)
                Next
                For Each foundFile As String In My.Computer.FileSystem.GetFiles("Assets\Fallout Songs_Mono\")
                    Dim FileName As String = Path.GetFileName(foundFile)
                    lbxMediaFalloutSongs.Items.Add(FileName)
                Next
            End If

            For X As Integer = 0 To lbxMediaFalloutSongs.Items.Count - 1
                If lbxMediaFalloutSongs.Items(X) = "desktop.ini" Then
                    lbxMediaFalloutSongs.Items.RemoveAt(X)
                    Exit For
                End If
            Next
        End If

        DashwoodP = txbDashwoodPercent.Text

        If rdbDJ_ThreeDog.Checked Then
            DJ = "ThreeDog"
        End If

        CreateNewsList()

    End Sub

    Private Sub ChooseIntro_1()
        Dim RNGen As New Random
        Dim RandomNum As Integer

        Do
            If ckbBS.Checked Then
                RandomNum = RNGen.Next(0, 14)
            Else
                RandomNum = RNGen.Next(0, 12)
            End If

            MediaFile = lbxMediaIntros.Items(RandomNum)

            RandomNum = RNGen.Next(0, 100)
            If Media_FirstTimePlay = False Then
                Dim LastSongIndex As Integer = lbxMediaFalloutSongsPlayed.Items.Count - 1
                If lbxMediaIntrosSongs.Items.Contains(lbxMediaFalloutSongsPlayed.Items(LastSongIndex)) And RandomNum >= 60 Then
                    MediaFile = lbxMediaFalloutSongsPlayed.Items(LastSongIndex)
                End If
            End If

            If lbxMediaIntrosPlayed.Items.Contains(MediaFile) Then
                Media_IntroRepeat = True
            Else
                Media_IntroRepeat = False
            End If

        Loop Until Media_IntroRepeat = False

        If lbxMediaIntrosPlayed.Items.Count >= 7 Then
            lbxMediaIntrosPlayed.Items.RemoveAt(0)
            lbxMediaIntrosPlayed.Items.Add(MediaFile.ToString)
        Else
            lbxMediaIntrosPlayed.Items.Add(MediaFile.ToString)
        End If

        If MediaFile.Substring(0, 3) = "mus" Or MediaFile.Substring(0, 3) = "mon" Then
            AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\" & DJ & "\IntroMusic\" & MediaFile))
        Else
            AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\" & DJ & "\Intros\" & MediaFile & ".mp3"))
        End If
        Media_FirstTimePlay = False

        CodeStage += 1
        CodeName = "Intro Chosen"
        CodeExtra = MediaFile
        DebugLogging()
    End Sub

    Private Sub ChooseDashwood()
        CodeStage += 1

        AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\Dashwood\Intro.mp3"))
        If DashwoodStage = 1 Then
            AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\Dashwood\1.mp3"))
            DashwoodStage = 2
        ElseIf DashwoodStage = 2 Then
            AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\Dashwood\2.mp3"))
            DashwoodStage = 3
        ElseIf DashwoodStage = 3 Then
            AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\Dashwood\3.mp3"))
            DashwoodStage = 4
        ElseIf DashwoodStage = 4 Then
            AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\Dashwood\4.mp3"))
            DashwoodStage = 1
        Else
            AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\" & DJ & "\" & "Silence.mp3"))
            DashwoodStage = 1
        End If
        AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\Dashwood\Exit.mp3"))
    End Sub

    Private Sub ChooseIntro_2()
        Dim RNGen As New Random
        Dim RandomNum As Integer

        Do
            If ckbBS.Checked Then
                RandomNum = RNGen.Next(0, 14)
            Else
                RandomNum = RNGen.Next(0, 12)
            End If

            MediaFile = lbxMediaIntros.Items(RandomNum)

            If lbxMediaIntrosPlayed.Items.Contains(MediaFile) Then
                Media_IntroRepeat = True
            Else
                Media_IntroRepeat = False
            End If

        Loop Until Media_IntroRepeat = False

        If lbxMediaIntrosPlayed.Items.Count >= 7 Then
            lbxMediaIntrosPlayed.Items.RemoveAt(0)
            lbxMediaIntrosPlayed.Items.Add(MediaFile.ToString)
        Else
            lbxMediaIntrosPlayed.Items.Add(MediaFile.ToString)
        End If

        'AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\" & DJ & "\Intros\" & MediaFile & ".mp3"))
        CodeStage += 1
    End Sub

    Private Sub ChooseNewsIntro()
        Dim RNGen As New Random
        Dim RandomNum As Integer

        Do
            If ckbBS.Checked Then
                RandomNum = RNGen.Next(0, 10)
            Else
                RandomNum = RNGen.Next(0, 6)
            End If

            MediaFile = lbxMediaNewsIntros.Items(RandomNum)

            If lbxMediaNewsIntrosPlayed.Items.Contains(MediaFile) Then
                Media_NewsIntroRepeat = True
            Else
                Media_NewsIntroRepeat = False
            End If

        Loop Until Media_NewsIntroRepeat = False

        If ckbBS.Checked = True And lbxMediaNewsIntrosPlayed.Items.Count >= 5 Then
            lbxMediaNewsIntrosPlayed.Items.RemoveAt(0)
            lbxMediaNewsIntrosPlayed.Items.Add(MediaFile.ToString)
        ElseIf ckbBS.Checked = False And lbxMediaNewsIntrosPlayed.Items.Count >= 3 Then
            lbxMediaNewsIntrosPlayed.Items.RemoveAt(0)
            lbxMediaNewsIntrosPlayed.Items.Add(MediaFile.ToString)
        Else
            lbxMediaNewsIntrosPlayed.Items.Add(MediaFile.ToString)
        End If

        AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\" & DJ & "\NewsIntro\" & MediaFile & ".mp3"))

        CodeStage += 1
        CodeName = "News Intro Chosen"
        CodeExtra = MediaFile
        DebugLogging()
    End Sub

    Private Sub ChooseNews()
        Dim RNGen As New Random
        Dim RandomNum As Integer

        Do
            RandomNum = RNGen.Next(0, lbxMediaNews.Items.Count)

            MediaFile = lbxMediaNews.Items(RandomNum)

            If lbxMediaNewsPlayed.Items.Contains(MediaFile) Then
                Media_NewsRepeat = True
            Else
                Media_NewsRepeat = False
            End If

        Loop Until Media_NewsRepeat = False

        If lbxMediaNewsPlayed.Items.Count >= 5 Then
            lbxMediaNewsPlayed.Items.RemoveAt(0)
            lbxMediaNewsPlayed.Items.Add(MediaFile.ToString)
        Else
            lbxMediaNewsPlayed.Items.Add(MediaFile.ToString)
        End If

        If (ckbRandomPlayerStats.Checked Or rdbKarmaRND.Checked Or rdbGenderRandom.Checked Or PlayerLevelRandom = True) And ckbPlayerStatsEnable.Checked Then
            RandomizePlayerStats()
        End If

        If MediaFile.Substring(0, 2) = "MQ" Then

            If My.Computer.FileSystem.FileExists("Assets\" & DJ & "\Player\" & PlayerKarma & "\" & PlayerGender & "\" & PlayerLevelS & ".mp3") And ckbPlayerStatsEnable.Checked Then
                AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\" & DJ & "\Player\" & PlayerKarma & "\" & PlayerGender & "\" & PlayerLevelS & ".mp3"))
            ElseIf My.Computer.FileSystem.FileExists("Assets\" & DJ & "\Player\" & PlayerKarma & "\B\" & PlayerLevelS & ".mp3") And ckbPlayerStatsEnable.Checked Then
                AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\" & DJ & "\Player\" & PlayerKarma & "\B\" & PlayerLevelS & ".mp3"))
            Else
            End If

            CodeStage += 1
            CodeName = "Player Quest Audio"
            CodeExtra = "Assets\" & DJ & "\Player\" & PlayerKarma & "\B\" & PlayerLevelS & ".mp3"
            DebugLogging()

            AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\" & DJ & "\MQuest\" & MediaFile & ".mp3"))

        ElseIf MediaFile.Substring(0, 2) = "Q_" Then

            If My.Computer.FileSystem.FileExists("Assets\" & DJ & "\Player\" & PlayerKarma & "\" & PlayerGender & "\" & PlayerLevelS & ".mp3") And ckbPlayerStatsEnable.Checked Then
                AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\" & DJ & "\Player\" & PlayerKarma & "\" & PlayerGender & "\" & PlayerLevelS & ".mp3"))
            ElseIf My.Computer.FileSystem.FileExists("Assets\" & DJ & "\Player\" & PlayerKarma & "\B\" & PlayerLevelS & ".mp3") And ckbPlayerStatsEnable.Checked Then
                AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\" & DJ & "\Player\" & PlayerKarma & "\B\" & PlayerLevelS & ".mp3"))
            Else
            End If

            CodeStage += 1
            CodeName = "Player Quest Audio"
            CodeExtra = "Assets\" & DJ & "\Player\" & PlayerKarma & "\B\" & PlayerLevelS & ".mp3"
            DebugLogging()

            AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\" & DJ & "\Quest\" & MediaFile & ".mp3"))

        ElseIf MediaFile = "Oasis" Or MediaFile = "Those" Or MediaFile = "TenpennyTower" Or MediaFile = "TheSuperhumanGambit" Then
            AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\" & DJ & "\Quest\" & MediaFile & ".mp3"))
        Else
            AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\" & DJ & "\News\" & MediaFile & ".mp3"))
        End If

        CodeStage += 1
        CodeName = "News Chosen"
        CodeExtra = MediaFile
        DebugLogging()
    End Sub

    Private Sub RandomizePlayerStats()
        Dim RNGen As New Random
        Dim RandomNum As Integer

        If ckbRandomPlayerStats.Checked Or rdbKarmaRND.Checked Then
            RandomNum = RNGen.Next(0, 100)
            If RandomNum <= 34 Then
                PlayerKarma = "E"
            ElseIf RandomNum >= 34 And RandomNum <= 66 Then
                PlayerKarma = "N"
            Else
                PlayerKarma = "G"
            End If
        End If

        If ckbRandomPlayerStats.Checked Or rdbGenderRandom.Checked Then
            RandomNum = RNGen.Next(0, 100)
            If RandomNum <= 50 Then
                PlayerGender = "M"
            Else
                PlayerGender = "F"
            End If
        End If

        If (ckbRandomPlayerStats.Checked Or PlayerLevelRandom = True) And ckbBS.Checked Then
            RandomNum = RNGen.Next(2, 30)
            PlayerLevel = RandomNum
            If PlayerLevel < 10 Then
                PlayerLevelS = "0" & PlayerLevel
            Else
                PlayerLevelS = PlayerLevel
            End If
        ElseIf ckbRandomPlayerStats.Checked Or PlayerLevelRandom = True Then
            RandomNum = RNGen.Next(2, 20)
            PlayerLevel = RandomNum
            If PlayerLevel < 10 Then
                PlayerLevelS = "0" & PlayerLevel
            Else
                PlayerLevelS = PlayerLevel
            End If
        End If

        CodeStage += 1
        CodeName = "Randomized Player Stats"
        CodeExtra = "Karma: " & PlayerKarma & " | Gender: " & PlayerGender & " | Level: " & PlayerLevel
        DebugLogging()
    End Sub

    Private Sub ChooseOutro_1()
        Dim RNGen As New Random
        Dim RandomNum As Integer

        Do
            If ckbBS.Checked Then
                RandomNum = RNGen.Next(0, 4)
            Else
                RandomNum = RNGen.Next(0, 4)
            End If

            MediaFile = lbxMediaOutros.Items(RandomNum)

            If lbxMediaOutrosPlayed.Items.Contains(MediaFile) Then
                Media_OutroRepeat = True
            Else
                Media_OutroRepeat = False
            End If

        Loop Until Media_OutroRepeat = False

        If lbxMediaOutrosPlayed.Items.Count >= 2 Then
            lbxMediaOutrosPlayed.Items.RemoveAt(0)
            lbxMediaOutrosPlayed.Items.Add(MediaFile.ToString)
        Else
            lbxMediaOutrosPlayed.Items.Add(MediaFile.ToString)
        End If
        Media_OutroRepeat = True

        AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\" & DJ & "\Outro\" & MediaFile & ".mp3"))

        CodeStage += 1
        CodeName = "1st Outro Chosen"
        CodeExtra = MediaFile
        DebugLogging()
    End Sub

    Private Sub ChoosePSAIntro()
        Dim RNGen As New Random
        Dim RandomNum As Integer

        Do
            If ckbBS.Checked Then
                RandomNum = RNGen.Next(0, 4)
            Else
                RandomNum = RNGen.Next(0, 4)
            End If

            MediaFile = lbxMediaPSAIntros.Items(RandomNum)

            If lbxMediaPSAIntrosPlayed.Items.Contains(MediaFile) Then
                Media_PSAIntroRepeat = True
            Else
                Media_PSAIntroRepeat = False
            End If

        Loop Until Media_PSAIntroRepeat = False

        If lbxMediaPSAIntrosPlayed.Items.Count >= 2 Then
            lbxMediaPSAIntrosPlayed.Items.RemoveAt(0)
            lbxMediaPSAIntrosPlayed.Items.Add(MediaFile.ToString)
        Else
            lbxMediaPSAIntrosPlayed.Items.Add(MediaFile.ToString)
        End If

        AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\" & DJ & "\PSAIntro\" & MediaFile & ".mp3"))

        CodeStage += 1
        CodeName = "Chose PSA Intro"
        CodeExtra = MediaFile
        DebugLogging()
    End Sub

    Private Sub ChoosePSA()
        Dim RNGen As New Random
        Dim RandomNum As Integer

        Do
            If ckbBS.Checked Then
                RandomNum = RNGen.Next(0, 6)
            Else
                RandomNum = RNGen.Next(0, 6)
            End If

            MediaFile = lbxMediaPSAs.Items(RandomNum)

            If lbxMediaPSAPlayed.Items.Contains(MediaFile) Then
                Media_PSARepeat = True
            Else
                Media_PSARepeat = False
            End If

        Loop Until Media_PSARepeat = False

        If lbxMediaPSAPlayed.Items.Count >= 3 Then
            lbxMediaPSAPlayed.Items.RemoveAt(0)
            lbxMediaPSAPlayed.Items.Add(MediaFile.ToString)
        Else
            lbxMediaPSAPlayed.Items.Add(MediaFile.ToString)
        End If

        AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\" & DJ & "\PSA\" & MediaFile & ".mp3"))

        CodeStage += 1
        CodeName = "Chose PSA"
        CodeExtra = MediaFile
        DebugLogging()
    End Sub

    Private Sub ChooseOutro_2() 'This Outro will include Song-Related Outros
        Dim RNGen As New Random
        Dim RandomNum As Integer

        CodeStage += 1
        CodeName = "2nd Outro Enter"
        CodeExtra = ""
        DebugLogging()

        Do
            If ckbBS.Checked Then
                RandomNum = RNGen.Next(0, 4)
            Else
                RandomNum = RNGen.Next(0, 4)
            End If

            CodeStage += 1
            CodeName = "2nd Outro RNG"
            CodeExtra = RandomNum
            DebugLogging()

            MediaFile = lbxMediaOutros.Items(RandomNum)

            CodeStage += 1
            CodeName = "2nd Outro Item"
            CodeExtra = MediaFile
            DebugLogging()

            If lbxMediaOutrosSongs.Items.Contains(lbxMediaFalloutSongQue.Items(0)) Then

                CodeStage += 1
                CodeName = "2nd Outro Music"
                CodeExtra = ""
                DebugLogging()

                RandomNum = RNGen.Next(0, 100)
                If RandomNum >= 40 Then
                    MediaFile = lbxMediaFalloutSongQue.Items(0)
                End If
            End If



            If lbxMediaOutrosPlayed.Items.Contains(MediaFile) Then
                CodeStage += 1
                CodeName = "2nd Outro Repeat?"
                CodeExtra = "Repeat is True"
                DebugLogging()
                Media_IntroRepeat = True
            Else
                CodeStage += 1
                CodeName = "2nd Outro Repeat?"
                CodeExtra = "Repeat is False, Send"
                DebugLogging()
                Media_IntroRepeat = False
                Exit Do
            End If

        Loop Until Media_OutroRepeat = False

        If lbxMediaOutrosPlayed.Items.Count >= 3 Then
            lbxMediaOutrosPlayed.Items.RemoveAt(0)
            lbxMediaOutrosPlayed.Items.Add(MediaFile.ToString)
        Else
            lbxMediaOutrosPlayed.Items.Add(MediaFile.ToString)
        End If
        If MediaFile.Substring(0, 3) = "mus" Or MediaFile.Substring(0, 3) = "mon" Then
            AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\" & DJ & "\OutroMusic\" & MediaFile))
        Else
            AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\" & DJ & "\Outro\" & MediaFile & ".mp3"))
        End If

        CodeStage += 1
        CodeName = "2nd Outro Chosen"
        CodeExtra = MediaFile
        DebugLogging()
    End Sub

    Private Sub ChooseSongs()
        Dim RNGen As New Random
        Dim RandomNum As Integer

        Do
            RandomNum = RNGen.Next(0, lbxMediaFalloutSongs.Items.Count)

            MediaFile = lbxMediaFalloutSongs.Items(RandomNum)

            If lbxMediaFalloutSongsPlayed.Items.Contains(MediaFile) Or lbxMediaFalloutSongQue.Items.Contains(MediaFile) Then
                Media_SongRepeat = True
            Else
                Media_SongRepeat = False
            End If

        Loop Until Media_SongRepeat = False

        lbxMediaFalloutSongQue.Items.Add(MediaFile)

        CodeStage += 1
        CodeName = "Chose a Song"
        CodeExtra = MediaFile
        DebugLogging()

        'If lbxMediaFalloutSongsPlayed.Items.Count >= 10 Then
        '    lbxMediaFalloutSongsPlayed.Items.RemoveAt(0)
        '    lbxMediaFalloutSongsPlayed.Items.Add(MediaFile.ToString)
        'Else
        '    lbxMediaFalloutSongsPlayed.Items.Add(MediaFile.ToString)
        'End If

        'If MediaFile.Substring(0, 4) = "mono" Then
        '    AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\Fallout Songs_Mono\" & MediaFile))
        'ElseIf MediaFile.Substring(0, 4) = "mus_" Then
        '    AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\Fallout Songs\" & MediaFile))
        'Else

        'End If
    End Sub

    Private Sub PlaySongs()
        If lbxMediaFalloutSongsPlayed.Items.Count >= 10 Then
            For X As Integer = 0 To lbxMediaFalloutSongQue.Items.Count - 1
                lbxMediaFalloutSongsPlayed.Items.RemoveAt(0)
                lbxMediaFalloutSongsPlayed.Items.Add(lbxMediaFalloutSongQue.Items(X))
            Next X
        Else
            For X As Integer = 0 To lbxMediaFalloutSongQue.Items.Count - 1
                lbxMediaFalloutSongsPlayed.Items.Add(lbxMediaFalloutSongQue.Items(X))
            Next X
        End If

        For X As Integer = 0 To lbxMediaFalloutSongQue.Items.Count - 1
            If lbxMediaFalloutSongQue.Items(X).Substring(0, 4) = "mono" Then
                AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\Fallout Songs_Mono\" & lbxMediaFalloutSongQue.Items(X)))
            ElseIf lbxMediaFalloutSongQue.Items(X).Substring(0, 4) = "mus_" Then
                AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\Fallout Songs\" & lbxMediaFalloutSongQue.Items(X)))
            Else
                AxWindowsMediaPlayer1.currentPlaylist.appendItem(AxWindowsMediaPlayer1.newMedia("Assets\Music\" & lbxMediaFalloutSongQue.Items(X)))
            End If
        Next

        lbxMediaFalloutSongQue.Items.Clear()

        CodeStage += 1
        CodeName = "Sent songs to play"
        CodeExtra = ""
        DebugLogging()
    End Sub

    Private Sub CreateNewsList()
        lbxMediaNews.Items.Clear()

        Dim GenNews() As String = {"GenNews01", "GenNews02", "GenNews03", "GenNews04", "GenNews05"}
        'Dim DLCNews() As String = {""}

        GetMainQuestCompleted()
        GetSideQuestCompleted()

        For X As Integer = 0 To lbxSideQuestsCompleted.Items.Count - 1
            lbxMediaNews.Items.Add(lbxSideQuestsCompleted.Items(X))
        Next X

        If ckbBS.Checked = True Then
            lbxMediaNews.Items.AddRange(GenNews)
            'lbxMediaNews.Items.AddRange(DLCNews)
            lbxMediaNews.Items.Add(MainQuestCompleted)
        Else
            lbxMediaNews.Items.AddRange(GenNews)
            lbxMediaNews.Items.Add(MainQuestCompleted)
        End If

        CodeStage += 1
        CodeName = "Created the News List"
        CodeExtra = ""
        DebugLogging()
    End Sub
    Private Sub GetMainQuestCompleted()
        If rdbMQuestE.Checked Then 'Escape!
            MainQuestCompleted = "MQuestEs"
        ElseIf rdbMQuestFiHF.Checked Then 'Following in His Footsteps
            MainQuestCompleted = "MQuestFiHF_" & PlayerGender
        ElseIf rdbMQuestTL.Checked Then 'Tranquility Lane
            MainQuestCompleted = "MQuestTL_" & PlayerGender
        ElseIf rdbMQuestTWoL.Checked Then 'The Waters of Life
            MainQuestCompleted = "MQuestTWoL"
        ElseIf rdbMQuestFtGoE.Checked Then 'Finding the Garden of Eden
            MainQuestCompleted = "MQuestFtGoE_" & PlayerGender
        ElseIf rdbMQuestTAD.Checked Then 'The American Dream
            If rdbMQuestTADGoodDie.Checked Then
                MainQuestCompleted = "MQuestTADDieGood"
            ElseIf rdbMQuestTADGoodLive.Checked Then
                MainQuestCompleted = "MQuestTADAliveGood"
            ElseIf rdbMQuestTADEvilDie.Checked Then
                MainQuestCompleted = "MQuestTADDieEvil_" & PlayerGender
            ElseIf rdbMQuestTADEvilLive.Checked Then
                MainQuestCompleted = "MQuestTADAliveEvil_" & PlayerGender
            End If
        ElseIf rdbMQuestTIB.Checked Then 'Take It Back!
            If ckbMQuest_BS_PI.Checked Then
                MainQuestCompleted = "MQuestTIBPI"
            Else
                MainQuestCompleted = "MQuestTIB"
            End If
        ElseIf rdbMQuestDFA.Checked Then 'Death From Above
            MainQuestCompleted = "MQuestDFA"
        ElseIf rdbMQuestSV.Checked Then 'Shock Value
            MainQuestCompleted = "MQuestSV"
        ElseIf rdbMQuestWDW.Checked Then 'Who Dares Wins
            If rdbMQuestWDWGood.Checked Then
                MainQuestCompleted = "MQuestWDWGood"
            ElseIf rdbMQuestWDWEvil.Checked Then
                MainQuestCompleted = "MQuestWDWEvil"
            End If
        Else
        End If
    End Sub
    Private Sub GetSideQuestCompleted()
        lbxSideQuestsCompleted.Items.Clear()

        If cblQuests.GetItemCheckState(0) Then 'Agatha's Song
            If rdbQuest_AS_Good.Checked Then
                lbxSideQuestsCompleted.Items.Add("Q_AS_Good")
            ElseIf rdbQuest_AS_Neutral.Checked Then
                lbxSideQuestsCompleted.Items.Add("Q_AS_Neutral")
            ElseIf rdbQuest_AS_Evil.Checked Then
                lbxSideQuestsCompleted.Items.Add("Q_AS_Evil")
            End If
        End If

        If cblQuests.GetItemCheckState(1) Then 'Big Trouble in Big Town
            If rdbQuest_BTiBT_Good.Checked Then
                lbxSideQuestsCompleted.Items.Add("Q_BTiBT_Good_" & PlayerGender)
            ElseIf rdbQuest_BTiBT_Evil.Checked Then
                lbxSideQuestsCompleted.Items.Add("Q_BTiBT_Evil")
            End If
        End If

        If cblQuests.GetItemCheckState(2) Then 'Blood Ties
            lbxSideQuestsCompleted.Items.Add("Q_BT_" & PlayerGender)
        End If

        If cblQuests.GetItemCheckState(3) Then 'Head of State
            If rdbQuest_HoS_Good.Checked Then
                lbxSideQuestsCompleted.Items.Add("Q_HoS_Good_" & PlayerGender)
            ElseIf rdbQuest_HoS_Evil.Checked Then
                lbxSideQuestsCompleted.Items.Add("Q_HoS_Evil")
            End If
        End If

        If cblQuests.GetItemCheckState(4) Then 'Oasis
            If rdbQuest_O_Evil.Checked Then
                lbxSideQuestsCompleted.Items.Add("Q_O_Evil")
            Else
                lbxSideQuestsCompleted.Items.Add("Oasis")
            End If
        Else
            lbxSideQuestsCompleted.Items.Add("Oasis")
        End If

        If cblQuests.GetItemCheckState(5) Then 'Reily's Rangers
            If rdbQuest_RR_Good.Checked Then
                lbxSideQuestsCompleted.Items.Add("Q_RR_Good")
            ElseIf rdbQuest_RR_Evil.Checked Then
                lbxSideQuestsCompleted.Items.Add("Q_RR_Evil")
            End If
        End If

        If cblQuests.GetItemCheckState(6) Then 'Rescue from Paradise Falls
            If rdbQuest_RfPF_Good.Checked Then
                lbxSideQuestsCompleted.Items.Add("Q_RfPF_Good_" & PlayerGender)
            ElseIf rdbQuest_RfPF_Neutral.Checked Then
                lbxSideQuestsCompleted.Items.Add("Q_RfPF_Neutral")
            End If
        End If

        If cblQuests.GetItemCheckState(7) Then 'Stealing Independence
            lbxSideQuestsCompleted.Items.Add("Q_SI_" & PlayerGender)
        End If

        If cblQuests.GetItemCheckState(8) Then 'Strictly Business
            lbxSideQuestsCompleted.Items.Add("Q_SB_" & PlayerGender)
        End If

        If cblQuests.GetItemCheckState(9) Then 'Tenpenny Tower
            If rdbQuest_TT_Good.Checked Then
                lbxSideQuestsCompleted.Items.Add("Q_TT_Good")
            Else
                lbxSideQuestsCompleted.Items.Add("Q_TT_Evil")
            End If
        Else
            lbxSideQuestsCompleted.Items.Add("TenpennyTower")
        End If

        If cblQuests.GetItemCheckState(10) Then 'The Nuka-Cola Challenge
            lbxSideQuestsCompleted.Items.Add("Q_TNCC")
        End If

        If cblQuests.GetItemCheckState(11) Then 'The Power of the Atom
            If rdbQuest_TPotA_Good.Checked Then
                lbxSideQuestsCompleted.Items.Add("Q_TPotA_Good")
            Else
                lbxSideQuestsCompleted.Items.Add("Q_TPotA_Evil")
            End If
        End If

        If cblQuests.GetItemCheckState(12) Then 'The Replicated Man
            If rdbQuest_TRM_Neutral.Checked Then
                lbxSideQuestsCompleted.Items.Add("Q_TRM_Neutral")
            ElseIf rdbQuest_TRM_Evil.Checked Then
                lbxSideQuestsCompleted.Items.Add("Q_TRM_Evil")
            End If
        End If

        If cblQuests.GetItemCheckState(13) Then 'The Superhuman Gambit
        Else
            lbxSideQuestsCompleted.Items.Add("TheSuperhumanGambit")
        End If

        If cblQuests.GetItemCheckState(14) Then 'Those!
            If rdbQuest_T_Good.Checked Then
                lbxSideQuestsCompleted.Items.Add("Q_T_Good_" & PlayerGender)
            ElseIf rdbQuest_T_Neutral.Checked Then
                lbxSideQuestsCompleted.Items.Add("Q_T_Neutral_" & PlayerGender)
            ElseIf rdbQuest_T_Evil.Checked Then
                lbxSideQuestsCompleted.Items.Add("Q_T_Evil_" & PlayerGender)
            End If
        Else
            lbxSideQuestsCompleted.Items.Add("Those")
        End If

        If cblQuests.GetItemCheckState(15) Then 'Trouble on the Homefront
            lbxSideQuestsCompleted.Items.Add("Q_TotH_" & PlayerGender)
        End If

        If cblQuests.GetItemCheckState(16) Then 'The Wasteland Survival Guide
            If rdbQuest_WSG_Good.Checked Then
                If rdbQuest_TPotA_Evil.Checked Then
                    lbxSideQuestsCompleted.Items.Add("Q_WSG_GoodB")
                Else
                    lbxSideQuestsCompleted.Items.Add("Q_WSG_GoodA")
                End If
            ElseIf rdbQuest_WSG_Evil.Checked Then
                lbxSideQuestsCompleted.Items.Add("Q_WSG_Evil_" & PlayerGender)
            End If
        End If

        If cblQuests.GetItemCheckState(17) And rdbQuest_YGSEitH_Evil.Checked Then 'You Gotta Shoot Em' in the Head
            lbxSideQuestsCompleted.Items.Add("Q_YGSEitH_Evil_" & PlayerGender)
        End If

    End Sub


    Private Sub CkbBS_CheckedChanged(sender As Object, e As EventArgs) Handles ckbBS.CheckedChanged
        If ckbBS.Checked Then


            rdbMQuestTIB.Enabled = True
            rdbMQuestDFA.Enabled = True
            rdbMQuestSV.Enabled = True
            rdbMQuestWDW.Enabled = True

            cmbLevel.Items.AddRange({21, 22, 23, 24, 25, 26, 27, 28, 29, 30})

        Else
            rdbMQuestTIB.Enabled = False
            grbMQuest_BS_TiBPI.Enabled = False
            rdbMQuestDFA.Enabled = False
            rdbMQuestSV.Enabled = False
            rdbMQuestWDW.Enabled = False
            grbMQuestWDW.Enabled = False

            rdbMQuestTIB.Checked = False
            rdbMQuestDFA.Checked = False
            rdbMQuestSV.Checked = False
            rdbMQuestWDW.Checked = False

            rdbMQuestE.Checked = True

            cmbLevel.Items.Remove(21)
            cmbLevel.Items.Remove(22)
            cmbLevel.Items.Remove(23)
            cmbLevel.Items.Remove(24)
            cmbLevel.Items.Remove(25)
            cmbLevel.Items.Remove(26)
            cmbLevel.Items.Remove(27)
            cmbLevel.Items.Remove(28)
            cmbLevel.Items.Remove(29)
            cmbLevel.Items.Remove(30)
        End If


    End Sub


    Private Sub rdbMQuestTAD_CheckedChanged(sender As Object, e As EventArgs) Handles rdbMQuestTAD.CheckedChanged
        If rdbMQuestTAD.Checked = True Then
            grbMQuestTAD.Enabled = True
        Else
            grbMQuestTAD.Enabled = False
        End If
    End Sub
    Private Sub rdbMQuestTIB_CheckedChanged(sender As Object, e As EventArgs) Handles rdbMQuestTIB.CheckedChanged
        If rdbMQuestTIB.Checked = True Then
            grbMQuest_BS_TiBPI.Enabled = True
        Else
            grbMQuest_BS_TiBPI.Enabled = False
        End If
    End Sub
    Private Sub rdbMQuestWDW_CheckedChanged(sender As Object, e As EventArgs) Handles rdbMQuestWDW.CheckedChanged
        If rdbMQuestWDW.Checked Then
            grbMQuestWDW.Enabled = True
        Else
            grbMQuestWDW.Enabled = False
        End If
    End Sub
    Private Sub cblQuests_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cblQuests.SelectedIndexChanged
        If cblQuests.GetItemCheckState(0) Then 'Agatha's Song
            grbQuest_AS.Enabled = True
        Else
            grbQuest_AS.Enabled = False
        End If
        If cblQuests.GetItemCheckState(1) Then 'Big Trouble in Big Town
            grbQuest_BTiBT.Enabled = True
        Else
            grbQuest_BTiBT.Enabled = False
        End If
        If cblQuests.GetItemCheckState(3) Then 'Head of State
            grbQuest_HoS.Enabled = True
        Else
            grbQuest_HoS.Enabled = False
        End If
        If cblQuests.GetItemCheckState(4) Then 'Oasis
            grbQuest_O.Enabled = True
        Else
            grbQuest_O.Enabled = False
        End If
        If cblQuests.GetItemCheckState(5) Then 'Reily's Rangers
            grbQuest_RR.Enabled = True
        Else
            grbQuest_RR.Enabled = False
        End If
        If cblQuests.GetItemCheckState(6) Then 'Rescue from Paradise Falls
            grbQuest_RfPF.Enabled = True
        Else
            grbQuest_RfPF.Enabled = False
        End If
        If cblQuests.GetItemCheckState(9) Then 'Tenpenny Tower
            grbQuest_TT.Enabled = True
        Else
            grbQuest_TT.Enabled = False
        End If
        If cblQuests.GetItemCheckState(11) Then 'The Power of the Atom
            grbQuest_TPotA.Enabled = True
        Else
            grbQuest_TPotA.Enabled = False
        End If
        If cblQuests.GetItemCheckState(12) Then 'The Replicated Man
            grbQuest_TRM.Enabled = True
        Else
            grbQuest_TRM.Enabled = False
        End If
        If cblQuests.GetItemCheckState(14) Then 'Those!
            grbQuest_T.Enabled = True
        Else
            grbQuest_T.Enabled = False
        End If
        If cblQuests.GetItemCheckState(16) Then 'The Wasteland Survival Guide
            grbQuest_WSG.Enabled = True
        Else
            grbQuest_WSG.Enabled = False
        End If
        If cblQuests.GetItemCheckState(17) Then 'You Gotta Shoot Em' in the Head
            grbQuest_YGSEitH.Enabled = True
        Else
            grbQuest_YGSEitH.Enabled = False
        End If
    End Sub

    '==========================================================================================================================================================================================


    Private Sub DebugLogging()

        'SysTime = DateTime.Now.ToString("HH:mm:ss:fffff")
        'IO.File.AppendAllText("Debug Log.txt", SysTime & " " & "Stage: " & CodeStage & " " & CodeName & " | " & CodeExtra & vbCr)


    End Sub

    Private Sub cmdSongFolder_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub AxWindowsMediaPlayer1_PlayStateChanged(sender As Object, e As AxWMPLib._WMPOCXEvents_PlayStateChangeEvent) Handles AxWindowsMediaPlayer1.PlayStateChange
        WMPState = e.newState
        lblWMPState.Text = WMPState
        'IO.File.AppendAllText("Assets\Test.txt", WMPState & " " & AxWindowsMediaPlayer1.currentMedia.name.ToString & vbCr)

        'lbxDebug.Items.Add(WMPState & AxWindowsMediaPlayer1.currentMedia.name)

        'If lbxMediaFalloutSongs.Items.Contains(AxWindowsMediaPlayer1.currentMedia.name) Then
        '    Dim SongIndex As Integer = lbxMediaFalloutSongs.Items.IndexOf(AxWindowsMediaPlayer1.currentMedia.name)
        '    txbPlaying.Text = lbxMediaFalloutSongsTitles.Items(SongIndex)
        'End If
        If PlayingMedia = True Then
            If WMPState = 3 And AxWindowsMediaPlayer1.currentMedia.name = "Silence" Then
                MasterPlayLoop()
            End If

            CodeStage += 1
            CodeName = "Media Player State Changed to: " & WMPState
            CodeExtra = AxWindowsMediaPlayer1.currentMedia.name.ToString
            DebugLogging()
        End If



    End Sub

    Private Sub llbFalloutMusicMode_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles llbFalloutMusicMode.LinkClicked
        MessageBox.Show("The 'Mono' versions of the songs have a lower quality and an old-timey radio filter applied to them, better simulating the music as heard in-game" & vbCr & "The 'Clear' songs are what you would expect from normal music.")
    End Sub

    Private Sub txtSongCountPerMin_TextChanged(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtSongCountPerMin.KeyPress
        '97 - 122 = Ascii codes for simple letters
        '65 - 90  = Ascii codes for capital letters
        '48 - 57  = Ascii codes for numbers

        If Asc(e.KeyChar) <> 8 Then
            If Asc(e.KeyChar) < 49 Or Asc(e.KeyChar) > 57 Then
                e.Handled = True
            End If
        End If

        If txtSongCountPerMin.Text.Length = 0 Then
            SongMin = 0
        Else
            SongMin = txtSongCountPerMin.Text
        End If


    End Sub
    Private Sub txtSongCountPerMax_TextChanged(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txtSongCountPerMax.KeyPress
        '97 - 122 = Ascii codes for simple letters
        '65 - 90  = Ascii codes for capital letters
        '48 - 57  = Ascii codes for numbers

        If Asc(e.KeyChar) <> 8 Then
            If Asc(e.KeyChar) < 49 Or Asc(e.KeyChar) > 57 Then
                e.Handled = True
            End If
        End If



        CheckSongMax()
    End Sub

    Private Sub CheckSongMax()


        Do
            If txtSongCountPerMax.Text.Length = 0 Then
                SongMax = 0
            Else
                SongMax = txtSongCountPerMax.Text
            End If
        Loop Until txtSongCountPerMax.Text.Length > 0

        MsgBox(SongMin & SongMax & txtSongCountPerMax.Text.Length)

        If txtSongCountPerMax.Text.Length = 0 Then
            SongMax = 0
        Else
            SongMax = txtSongCountPerMax.Text
        End If

        ' MsgBox(SongMin & SongMax & txtSongCountPerMax.Text.Length)

        If txtSongCountPerMax.Text.Length > 0 Then

            If SongMax < SongMin Then
                MsgBox(SongMin & SongMax & txtSongCountPerMax.Text.Length)
                txtSongCountPerMax.Clear()
                txtSongCountPerMax.Text = txtSongCountPerMin.Text
            End If
        End If

    End Sub
    Private Sub txbDashwoodPercent_TextChanged(sender As Object, e As System.Windows.Forms.KeyPressEventArgs) Handles txbDashwoodPercent.KeyPress
        If Asc(e.KeyChar) <> 8 Then
            If Asc(e.KeyChar) < 48 Or Asc(e.KeyChar) > 57 Then
                e.Handled = True
            End If
        End If
    End Sub

    Private Sub ckbRandomPlayerStats_CheckedChanged(sender As Object, e As EventArgs) Handles ckbRandomPlayerStats.CheckedChanged
        If ckbRandomPlayerStats.Checked Then
            gbxPlayerStats.Enabled = False
        Else
            gbxPlayerStats.Enabled = True
        End If
    End Sub
    Private Sub ckbPlayerStatsEnable_CheckedChanged(sender As Object, e As EventArgs) Handles ckbPlayerStatsEnable.CheckedChanged
        If ckbPlayerStatsEnable.Checked Then
            gbxPlayerGender.Enabled = True
            gbxPlayerKarma.Enabled = True
            gbxPlayerLevel.Enabled = True
            ckbRandomPlayerStats.Enabled = True
        Else
            gbxPlayerGender.Enabled = False
            gbxPlayerKarma.Enabled = False
            gbxPlayerLevel.Enabled = False
            ckbRandomPlayerStats.Enabled = False
        End If
    End Sub

    Private Sub lkbAbout_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles lkbAbout.LinkClicked
        lbxVersionInfo.Items.Clear()
        txtVersionInfo.Clear()
        Dim VersionInfoFile As String = "Version.txt"
        Dim VersionInfo As String() = System.IO.File.ReadAllLines(VersionInfoFile, encodingASCII)
        lbxVersionInfo.Items.AddRange(VersionInfo)
        For X As Integer = 0 To lbxVersionInfo.Items.Count - 1
            txtVersionInfo.Text &= lbxVersionInfo.Items(X) & vbCrLf
        Next
        MessageBox.Show(txtVersionInfo.Text)
    End Sub

    Private Sub ckbDebug_CheckedChanged(sender As Object, e As EventArgs) Handles ckbDebug.CheckedChanged
        If ckbDebug.Checked Then
            gbxDebug.Visible = True
        Else
            gbxDebug.Visible = False
        End If
    End Sub
    Private Sub btnDebug_Click(sender As Object, e As EventArgs) Handles btnDebug.Click
        'Dim RNGen As New Random
        'Dim RandomNum As Integer

    End Sub

    Private Sub SaveFile()
        txtRWStats.Text = ckbPlayerStatsEnable.Checked & ";" & ckbRandomPlayerStats.Checked & ";" & PlayerKarma & ";" & PlayerGender & ";" & PlayerLevel & ";" & ckbBS.Checked

        txtRWQuests.Text = MainQuestCompleted & ";"
        For X As Integer = 0 To lbxSideQuestsCompleted.Items.Count - 1
            txtRWQuests.Text &= lbxSideQuestsCompleted.Items(X) & ";"
        Next
        If cblQuests.GetItemCheckState(4) And rdbQuest_O_Evil.Checked = False Then
            txtRWQuests.Text &= "Q_O_Good" & ";"
        End If
        If cblQuests.GetItemCheckState(13) = False Then
            txtRWQuests.Text &= "TSG" & ";"
        End If
        If cblQuests.GetItemCheckState(12) And rdbQuest_TRM_Neutral.Checked = False And rdbQuest_TRM_Evil.Checked = False Then
            txtRWQuests.Text &= "Q_TRM_Good" & ";"
        End If
        If cblQuests.GetItemCheckState(17) And rdbQuest_YGSEitH_Evil.Checked = False Then
            txtRWQuests.Text &= "Q_YGSEitH_Neutral" & ";"
        End If

        If rdbDJ_ThreeDog.Checked Then
            txtRWSettings.Text &= "ThreeDog" & ";"
        ElseIf rdbDJ_Margaret.Checked Then
            txtRWSettings.Text &= "Margaret" & ";"
        Else txtRWSettings.Text &= "Music" & ";"
        End If
        If rdbMusicModeClear.Checked Then
            txtRWSettings.Text &= "Clear" & ";"
        ElseIf rdbMusicModeMono.Checked Then
            txtRWSettings.Text &= "Mono" & ";"
        Else txtRWSettings.Text &= "Both" & ";"
        End If
        txtRWSettings.Text &= ckbDashwood.Checked & ";" & txbDashwoodPercent.Text & ";" & ckbOtherMusic.Checked & ";" & txtSongCountPerMin.Text & ";" & txtSongCountPerMax.Text & ";"




        SaveString = "GNRSaveFile" & vbCr & txtRWStats.Text & vbCr & txtRWQuests.Text & vbCr & txtRWSettings.Text
    End Sub

    Private Sub cmdSave_Click(sender As Object, e As EventArgs) Handles cmdSave.Click
        Dim append As Boolean = False

        txtRWStats.Clear()
        txtRWQuests.Clear()
        txtRWSettings.Clear()

        SetStats()

        SaveFile()

        'Dim objWriter As System.IO.StreamWriter
        'objWriter = New System.IO.StreamWriter("Configs\" & "Test.txt", append, encodingASCII)
        'objWriter.Write(SaveString)
        'objWriter.Close()


        SaveFileDialog1.Filter = "TXT Files (*.txt*)|*.txt"
        If SaveFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK _
         Then
            'My.Computer.FileSystem.WriteAllText() _
            '(SaveFileDialog1.FileName, SaveString, True)
            Dim objWriter As System.IO.StreamWriter
            objWriter = New System.IO.StreamWriter(SaveFileDialog1.FileName, append, encodingASCII)
            objWriter.Write(SaveString)
            objWriter.Close()

        End If
    End Sub

    Private Sub LoadFile()

        Dim LoadedRecord As String() = System.IO.File.ReadAllLines(LoadFileName, encodingASCII)
        lbxRWReadFile.Items.Clear()
        lbxRWReadFile.Items.AddRange(LoadedRecord)

        If lbxRWReadFile.Items(0) = "GNRSaveFile" Then

        Else
            MsgBox("Not a valid Config file")
            Exit Sub
        End If

        txtRWStats.Text = lbxRWReadFile.Items(1)
        lbxRWStats.Items.AddRange(txtRWStats.Text.Split({";"}, StringSplitOptions.RemoveEmptyEntries))

        If lbxRWStats.Items(0) = True Then
            ckbPlayerStatsEnable.Checked = True
        Else ckbPlayerStatsEnable.Checked = False
        End If
        If lbxRWStats.Items(1) = True Then
            ckbRandomPlayerStats.Checked = True
        Else ckbRandomPlayerStats.Checked = False
        End If
        If lbxRWStats.Items(2) = "E" Then
            rdbKarmaEvil.Checked = True
        ElseIf lbxRWStats.Items(2) = "N" Then
            rdbKarmaNeutral.Checked = True
        ElseIf lbxRWStats.Items(2) = "G" Then
            rdbKarmaGood.Checked = True
        Else rdbKarmaRND.Checked = True
        End If
        If lbxRWStats.Items(3) = "M" Then
            rdbGenderMale.Checked = True
        ElseIf lbxRWStats.Items(3) = "F" Then
            rdbGenderFemale.Checked = True
        Else rdbGenderRandom.Checked = True
        End If
        If lbxRWStats.Items(5) = True Then
            ckbBS.Checked = True
            'CkbBS_CheckedChanged(Nothing, Nothing)
        Else
            ckbBS.Checked = False
            'CkbBS_CheckedChanged(Nothing, Nothing)
        End If
        cmbLevel.SelectedIndex = lbxRWStats.Items(4).ToString - 1


        txtRWQuests.Text = lbxRWReadFile.Items(2)
        lbxRWQuests.Items.AddRange(txtRWQuests.Text.Split({";"}, StringSplitOptions.RemoveEmptyEntries))

        If lbxRWQuests.Items(0) = "MQuestEs" Then
            rdbMQuestE.Checked = True
        ElseIf lbxRWQuests.Items(0) = "MQuestFiHF_M" Or lbxRWQuests.Items(0) = "MQuestFiHF_F" Then
            rdbMQuestFiHF.Checked = True
        ElseIf lbxRWQuests.Items(0) = "MQuestTL_M" Or lbxRWQuests.Items(0) = "MQuestTL_F" Then
            rdbMQuestTL.Checked = True
        ElseIf lbxRWQuests.Items(0) = "MQuestTWoL" Then
            rdbMQuestTWoL.Checked = True
        ElseIf lbxRWQuests.Items(0) = "MQuestFtGoE_M" Or lbxRWQuests.Items(0) = "MQuestFtGoE_F" Then
            rdbMQuestFtGoE.Checked = True
        ElseIf lbxRWQuests.Items(0) = "MQuestTADDieGood" Then
            rdbMQuestTAD.Checked = True
            rdbMQuestTADGoodDie.Checked = True
            rdbMQuestTAD_CheckedChanged(Nothing, Nothing)
        ElseIf lbxRWQuests.Items(0) = "MQuestTADAliveGood" Then
            rdbMQuestTAD.Checked = True
            rdbMQuestTADGoodLive.Checked = True
            rdbMQuestTAD_CheckedChanged(Nothing, Nothing)
        ElseIf lbxRWQuests.Items(0) = "MQuestTADDieEvil_M" Or lbxRWQuests.Items(0) = "MQuestTADDieEvil_F" Then
            rdbMQuestTAD.Checked = True
            rdbMQuestTADEvilDie.Checked = True
            rdbMQuestTAD_CheckedChanged(Nothing, Nothing)
        ElseIf lbxRWQuests.Items(0) = "MQuestTADAliveEvil_M" Or lbxRWQuests.Items(0) = "MQuestTADAliveEvil_F" Then
            rdbMQuestTAD.Checked = True
            rdbMQuestTADEvilLive.Checked = True
            rdbMQuestTAD_CheckedChanged(Nothing, Nothing)
        ElseIf lbxRWQuests.Items(0) = "MQuestTIBPI" Then
            rdbMQuestTIB.Checked = True
            ckbMQuest_BS_PI.Checked = True
            rdbMQuestTIB_CheckedChanged(Nothing, Nothing)
        ElseIf lbxRWQuests.Items(0) = "MQuestTIB" Then
            rdbMQuestTIB.Checked = True
            rdbMQuestTIB_CheckedChanged(Nothing, Nothing)
        ElseIf lbxRWQuests.Items(0) = "MQuestDFA" Then
            rdbMQuestDFA.Checked = True
        ElseIf lbxRWQuests.Items(0) = "MQuestSV" Then
            rdbMQuestSV.Checked = True
        ElseIf lbxRWQuests.Items(0) = "MQuestWDWGood" Then
            rdbMQuestWDW.Checked = True
            rdbMQuestWDWGood.Checked = True
            rdbMQuestWDW_CheckedChanged(Nothing, Nothing)
        ElseIf lbxRWQuests.Items(0) = "MQuestWDWEvil" Then
            rdbMQuestWDW.Checked = True
            rdbMQuestWDWEvil.Checked = True
            rdbMQuestWDW_CheckedChanged(Nothing, Nothing)
        ElseIf lbxRWQuests.Items(0) = "" Then
            rdbMQuestNone.Checked = True
        End If

        SideQuestLoad()

        txtRWSettings.Text = lbxRWReadFile.Items(3)
        lbxRWSettings.Items.AddRange(txtRWSettings.Text.Split({";"}, StringSplitOptions.RemoveEmptyEntries))

        If lbxRWSettings.Items(0) = "ThreeDog" Then
            rdbDJ_ThreeDog.Checked = True
        ElseIf lbxRWSettings.Items(0) = "Margaret" Then
            rdbDJ_Margaret.Checked = True
        Else rdbDJ_None.Checked = True
        End If
        If lbxRWSettings.Items(1) = "Clear" Then
            rdbMusicModeClear.Checked = True
        ElseIf lbxRWSettings.Items(1) = "Mono" Then
            rdbMusicModeMono.Checked = True
        Else rdbMusicModeBoth.Checked = True
        End If
        If lbxRWSettings.Items(2) = True Then
            ckbDashwood.Checked = True
        Else ckbDashwood.Checked = False
        End If
        txbDashwoodPercent.Text = lbxRWSettings.Items(3)
        If lbxRWSettings.Items(4) = True Then
            ckbOtherMusic.Checked = True
        Else ckbOtherMusic.Checked = False
        End If
        txtSongCountPerMin.Text = lbxRWSettings.Items(5)
        txtSongCountPerMax.Text = lbxRWSettings.Items(6)


    End Sub
    Private Sub SideQuestLoad()
        If lbxRWQuests.Items.Contains("Q_AS_Good") Then
            cblQuests.SetItemChecked(0, True)
            rdbQuest_AS_Good.Checked = True
        ElseIf lbxRWQuests.Items.Contains("Q_AS_Neutral") Then
            cblQuests.SetItemChecked(0, True)
            rdbQuest_AS_Neutral.Checked = True
        ElseIf lbxRWQuests.Items.Contains("Q_AS_Evil") Then
            cblQuests.SetItemChecked(0, True)
            rdbQuest_AS_Evil.Checked = True
        Else
            cblQuests.SetItemChecked(0, False)
        End If

        If lbxRWQuests.Items.Contains("Q_BTiBT_Good_M") Or lbxRWQuests.Items.Contains("Q_BTiBT_Good_F") Then
            cblQuests.SetItemChecked(1, True)
            rdbQuest_BTiBT_Good.Checked = True
        ElseIf lbxRWQuests.Items.Contains("Q_BTiBT_Evil") Then
            cblQuests.SetItemChecked(1, True)
            rdbQuest_BTiBT_Evil.Checked = True
        Else
            cblQuests.SetItemChecked(1, False)
        End If

        If lbxRWQuests.Items.Contains("Q_BT_M") Or lbxRWQuests.Items.Contains("Q_BT_F") Then
            cblQuests.SetItemChecked(2, True)
        Else
            cblQuests.SetItemChecked(2, False)
        End If

        If lbxRWQuests.Items.Contains("Q_HoS_Good_M") Or lbxRWQuests.Items.Contains("Q_HoS_Good_F") Then
            cblQuests.SetItemChecked(3, True)
            rdbQuest_HoS_Good.Checked = True
        ElseIf lbxRWQuests.Items.Contains("Q_HoS_Evil") Then
            cblQuests.SetItemChecked(3, True)
            rdbQuest_HoS_Evil.Checked = True

        Else
            cblQuests.SetItemChecked(3, False)
        End If

        If lbxRWQuests.Items.Contains("Q_O_Evil") Then
            cblQuests.SetItemChecked(4, True)
            rdbQuest_O_Evil.Checked = True
        ElseIf lbxRWQuests.Items.Contains("Q_O_Good") Then
            cblQuests.SetItemChecked(4, True)
            rdbQuest_O_Neutral.Checked = True
        Else
            cblQuests.SetItemChecked(4, False)
        End If

        If lbxRWQuests.Items.Contains("Q_RR_Good") Then
            cblQuests.SetItemChecked(5, True)
            rdbQuest_RR_Good.Checked = True
        ElseIf lbxRWQuests.Items.Contains("Q_RR_Evil") Then
            cblQuests.SetItemChecked(5, True)
            rdbQuest_RR_Evil.Checked = True
        Else
            cblQuests.SetItemChecked(5, False)
        End If

        If lbxRWQuests.Items.Contains("Q_RfPF_Good_M") Or lbxRWQuests.Items.Contains("Q_RfPF_Good_F") Then
            cblQuests.SetItemChecked(6, True)
            rdbQuest_RfPF_Good.Checked = True
        ElseIf lbxRWQuests.Items.Contains("Q_RfPF_Neutral") Then
            cblQuests.SetItemChecked(6, True)
            rdbQuest_RfPF_Neutral.Checked = True
        Else
            cblQuests.SetItemChecked(6, False)
        End If

        If lbxRWQuests.Items.Contains("Q_SI_M") Or lbxRWQuests.Items.Contains("Q_SI_F") Then
            cblQuests.SetItemChecked(7, True)
        Else
            cblQuests.SetItemChecked(7, False)
        End If

        If lbxRWQuests.Items.Contains("Q_SB_M") Or lbxRWQuests.Items.Contains("Q_SB_F") Then
            cblQuests.SetItemChecked(8, True)
        Else
            cblQuests.SetItemChecked(8, False)
        End If

        If lbxRWQuests.Items.Contains("Q_TT_Good") Then
            cblQuests.SetItemChecked(9, True)
            rdbQuest_TT_Good.Checked = True
        ElseIf lbxRWQuests.Items.Contains("Q_TT_Evil") Then
            cblQuests.SetItemChecked(9, True)
            rdbQuest_TT_Evil.Checked = True
        Else
            cblQuests.SetItemChecked(9, False)
        End If

        If lbxRWQuests.Items.Contains("Q_TNCC") Then
            cblQuests.SetItemChecked(10, True)
        Else
            cblQuests.SetItemChecked(10, False)
        End If

        If lbxRWQuests.Items.Contains("Q_TPotA_Good") Then
            cblQuests.SetItemChecked(11, True)
            rdbQuest_TPotA_Good.Checked = True
        ElseIf lbxRWQuests.Items.Contains("Q_TPotA_Evil") Then
            cblQuests.SetItemChecked(11, True)
            rdbQuest_TPotA_Evil.Checked = True
        Else
            cblQuests.SetItemChecked(11, False)
        End If

        If lbxRWQuests.Items.Contains("Q_TRM_Neutral") Then
            cblQuests.SetItemChecked(12, True)
            rdbQuest_TRM_Neutral.Checked = True
        ElseIf lbxRWQuests.Items.Contains("Q_TRM_Evil") Then
            cblQuests.SetItemChecked(12, True)
            rdbQuest_TRM_Evil.Checked = True
        ElseIf lbxRWQuests.Items.Contains("Q_TRM_Good") Then
            cblQuests.SetItemChecked(12, True)
            rdbQuest_TRM_Good.Checked = True
        Else
            cblQuests.SetItemChecked(12, False)
        End If

        If lbxRWQuests.Items.Contains("TheSuperhumanGambit") Then
            cblQuests.SetItemChecked(13, False)
        Else
            cblQuests.SetItemChecked(13, True)
        End If

        If lbxRWQuests.Items.Contains("Q_T_Good_M") Or lbxRWQuests.Items.Contains("Q_T_Good_F") Then
            cblQuests.SetItemChecked(14, True)
            rdbQuest_T_Good.Checked = True
        ElseIf lbxRWQuests.Items.Contains("Q_T_Neutral_M") Or lbxRWQuests.Items.Contains("Q_T_Neutral_F") Then
            cblQuests.SetItemChecked(14, True)
            rdbQuest_T_Neutral.Checked = True
        ElseIf lbxRWQuests.Items.Contains("Q_T_Evil_M") Or lbxRWQuests.Items.Contains("Q_T_Evil_F") Then
            cblQuests.SetItemChecked(14, True)
            rdbQuest_T_Evil.Checked = True
        Else
            cblQuests.SetItemChecked(14, False)
        End If

        If lbxRWQuests.Items.Contains("Q_TotH_M") Or lbxRWQuests.Items.Contains("Q_TotH_F") Then
            cblQuests.SetItemChecked(15, True)
        Else
            cblQuests.SetItemChecked(15, False)
        End If

        If lbxRWQuests.Items.Contains("Q_WSG_GoodB") Or lbxRWQuests.Items.Contains("Q_WSG_GoodA") Then
            cblQuests.SetItemChecked(16, True)
            rdbQuest_WSG_Good.Checked = True
        ElseIf lbxRWQuests.Items.Contains("Q_WSG_Evil_M") Or lbxRWQuests.Items.Contains("Q_WSG_Evil_F") Then
            cblQuests.SetItemChecked(16, True)
            rdbQuest_WSG_Evil.Checked = True
        Else
            cblQuests.SetItemChecked(16, False)
        End If

        If lbxRWQuests.Items.Contains("Q_YGSEitH_Evil_M") Or lbxRWQuests.Items.Contains("Q_YGSEitH_Evil_F") Then
            cblQuests.SetItemChecked(17, True)
            rdbQuest_YGSEitH_Evil.Checked = True
        ElseIf lbxRWQuests.Items.Contains("Q_YGSEitH_Neutral") Then
            cblQuests.SetItemChecked(17, True)
            rdbQuest_YGSEitH_Neutral.Checked = True
        Else
            cblQuests.SetItemChecked(17, False)
        End If

        cblQuests_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Private Sub cmdLoad_Click(sender As Object, e As EventArgs) Handles cmdLoad.Click
        Dim LoadConfig As New OpenFileDialog
        LoadConfig.InitialDirectory = Application.StartupPath & "\Configs\"


        txtRWStats.Clear()
        lbxRWStats.Items.Clear()
        txtRWQuests.Clear()
        lbxRWQuests.Items.Clear()
        txtRWSettings.Clear()
        lbxRWSettings.Items.Clear()


        'OpenFileDialog1.InitialDirectory = "Configs\"
        If LoadConfig.ShowDialog = System.Windows.Forms.DialogResult.OK Then
            LoadFileName = LoadConfig.FileName
            LoadFile()
        End If
    End Sub

    Private Sub btnResetDefault_Click(sender As Object, e As EventArgs) Handles btnResetDefault.Click
        Dim append As Boolean = False
        Dim TrueDefault As String = "Configs\True-Default.txt"
        Dim DefaultFile As String = "Configs\Default.txt"
        Dim LoadedRecord As String() = System.IO.File.ReadAllLines(TrueDefault, encodingASCII)

        lbxRWReadFile.Items.Clear()
        lbxRWReadFile.Items.AddRange(LoadedRecord)


        Dim objWriter As System.IO.StreamWriter
        objWriter = New System.IO.StreamWriter(DefaultFile, append, encodingASCII)
        For X As Integer = 0 To lbxRWReadFile.Items.Count - 1
            objWriter.Write(lbxRWReadFile.Items(X) & vbCr)
        Next X

        objWriter.Close()
    End Sub

    Private Sub ckbCompactPlayer_CheckedChanged(sender As Object, e As EventArgs) Handles ckbCompactPlayer.CheckedChanged
        If ckbCompactPlayer.Checked = True And PlayingMedia = True Then
            grbSideQuestSettings.Visible = False
            grbSideQuests.Visible = False
            grbMainQuests.Visible = False
            gbxPlayerStats.Visible = False
            grbDJ.Visible = False
            grbMusicSelection.Visible = False
            grbSaveLoadButtons.Visible = False
        ElseIf ckbCompactPlayer.Checked = False And PlayingMedia = True Then
            grbSideQuestSettings.Visible = True
            grbSideQuests.Visible = True
            grbMainQuests.Visible = True
            gbxPlayerStats.Visible = True
            grbDJ.Visible = True
            grbMusicSelection.Visible = True
            grbSaveLoadButtons.Visible = True

        End If
    End Sub

    Private Sub ckbOtherMusic_CheckedChanged(sender As Object, e As EventArgs) Handles ckbOtherMusic.CheckedChanged
        If ckbOtherMusic.Checked Then
            ckbOtherMusicExclusive.Enabled = True
        Else
            ckbOtherMusicExclusive.Checked = False
            ckbOtherMusicExclusive.Enabled = False
        End If
    End Sub
End Class

