﻿Imports System.Windows.Media.Animation

Public Class LabWindow
    Inherits LabAssistantWindow

    Public Property SelectedReaction As Matter.Reaction
        Get
            Return selectedReaction_
        End Get
        Set(value As Matter.Reaction)
            If IsNothing(value) Then
                DeselectReaction()
                selectedReaction_ = Nothing
            Else
                SelectReaction(value)
                selectedReaction_ = value
            End If
        End Set
    End Property
    Private selectedReaction_ As Matter.Reaction

    Public Sub Initialize()
        SetElements(tableViewbox)
        handleMenuClick(tableMenuBtn, Nothing)
        timer1.Interval = TimeSpan.FromSeconds(0.7)
        timer2.Interval = TimeSpan.FromSeconds(0.7)
        timer3.Interval = TimeSpan.FromSeconds(0.7)
        timer4.Interval = TimeSpan.FromSeconds(0.7)
        versionBlock.Text = My.Application.Info.Version.ToString(3)
        Dim b As New Binding("AutoStartup")
        b.Source = My.Settings
        b.Mode = BindingMode.TwoWay
        startSelection.SetBinding(LabToggleButton.EnabledProperty, b)
        Dim b2 As New Binding("AutoStartupFile")
        b2.Source = My.Settings
        b2.Mode = BindingMode.TwoWay
        selectStartupLabel.SetBinding(Label.ContentProperty, b2)
        Dim b3 As New Binding("AskForConfirmationOnDelete")
        b3.Source = My.Settings
        b3.Mode = BindingMode.TwoWay

    End Sub

    Private Sub SetElements(inElement As DependencyObject)
        For Each o As Object In LogicalTreeHelper.GetChildren(inElement)

            If o.GetType().IsEquivalentTo(GetType(TableButton)) Then
                Dim tb As TableButton = o
                If tb.Tag IsNot Nothing Then
                    Dim use As Integer
                    If Integer.TryParse(tb.Tag, use) Then
                        tb.Element = Matter.Element.FromAtomicNumber(use)
                    Else
                        tb.PaintGroup([Enum].Parse(GetType(Matter.Element.Groups), tb.Tag))
                    End If

                End If

            Else
                If o.GetType().IsSubclassOf(GetType(DependencyObject)) Then
                    SetElements(o)
                End If
            End If
        Next
    End Sub

    Public Sub handleLaboratoryLoaded(sender As Laboratory, e As EventArgs)
        handleLabLoaded(sender, e)
        handleLabChanged(sender, e)
    End Sub

    Public Sub handleLaboratoryUnloaded(sender As Object, e As EventArgs)
        saveBtn.IsEnabled = False
        inventoryBtn.IsEnabled = False
    End Sub


    Private Sub handleLabLoaded(sender As Laboratory, e As EventArgs)
        saveBtn.IsEnabled = False
        inventoryBtn.IsEnabled = True
        AddHandler sender.LabChanged, labChangedDelegate
    End Sub

    Private labChangedDelegate As EventHandler(Of EventArgs) = AddressOf handleLabChanged
    Private Sub handleLabChanged(sender As Laboratory, e As EventArgs)
        Matter.Reaction.UpdateRecreatable()
        saveBtn.IsEnabled = True
        inventoryGrid.GetBindingExpression(DataGrid.ItemsSourceProperty).UpdateTarget()
        inventoryGrid.Items.Refresh()
    End Sub

#Region "Menu Selection"

    Private hidden As Boolean = False
    Private fullWidth As Double
    Private animating As Boolean = False
    Private WithEvents da As New DoubleAnimation(0, GetDuration(0.5))
    Private Sub handleMenuClick(sender As Object, e As RoutedEventArgs)
        Select Case sender.Tag
            Case Is = "settings"
                DeselectMenuItems(menuStackPanel, sender)
                tabDisplay.SelectedItem = startupSettingsPage
            Case Is = "table"
                DeselectMenuItems(menuStackPanel, sender)
                tabDisplay.SelectedItem = tableTabPage
            Case Is = "inorganic"
                DeselectMenuItems(menuStackPanel, sender)
                tabDisplay.SelectedItem = inorganicsPage
            Case Is = "organic"
                DeselectMenuItems(menuStackPanel, sender)
                tabDisplay.SelectedItem = organicsPage
            Case Is = "hide"
                If Not animating Then
                    If hidden Then
                        da.From = leftPanel.ActualWidth
                        da.To = fullWidth
                        leftPanel.BeginAnimation(Grid.WidthProperty, da)
                        animating = True
                        AnimateAllLabels(leftPanel, False)
                    Else
                        fullWidth = leftPanel.ActualWidth
                        da.From = leftPanel.ActualWidth
                        da.To = Me.FindResource("itemHeight") + 3.5
                        leftPanel.BeginAnimation(Grid.WidthProperty, da)
                        animating = True
                        AnimateAllLabels(leftPanel, True)
                    End If
                    hidden = Not hidden
                End If
            Case Is = "about"
                DeselectMenuItems(menuStackPanel, sender)
                tabDisplay.SelectedItem = aboutPage
            Case Is = "load"
                Dim o As New Microsoft.Win32.OpenFileDialog()
                o.Filter = OpenLabFilter
                o.Multiselect = False
                o.Title = "Load a Laboratory"
                o.CheckFileExists = True
                o.CheckPathExists = True

                If o.ShowDialog(Me) Then
                    Dim l As Laboratory = Laboratory.LoadFrom(o.FileName)
                    If Not IsNothing(l) Then
                        Matter.Info.LoadLab(l)
                    End If
                End If
            Case Is = "save"
                Dim s As New Microsoft.Win32.SaveFileDialog()
                s.Filter = Constants.OpenLabFilter
                s.AddExtension = True
                s.CheckPathExists = True
                s.DefaultExt = "*.clab"
                If s.ShowDialog(Me) Then
                    Try
                        RemoveHandler Matter.Info.LoadedLab.LabChanged, labChangedDelegate
                        If Laboratory.SaveTo(s.FileName, Matter.Info.LoadedLab) Then
                            MsgBox("Laboratory saved!")
                            saveBtn.IsEnabled = False
                        Else
                            Throw New Exception()
                        End If
                    Catch ex As Exception
                        MsgBox("An error occured!")
                    Finally
                        AddHandler Matter.Info.LoadedLab.LabChanged, labChangedDelegate
                    End Try
                Else
                    MsgBox("Not saved!")
                End If
            Case Is = "new"
                If saveBtn.IsEnabled Then
                    Dim dr As MsgBoxResult = MsgBox("You have modified the current laboratory." & vbNewLine & "Do you want to save first?", MsgBoxStyle.YesNoCancel)
                    Select Case dr
                        Case MsgBoxResult.Yes
                            saveBtn.PerformClick()
                        Case MsgBoxResult.No
                            createNewLab()
                    End Select
                Else
                    createNewLab()
                End If
            Case Is = "reaction"
                If Not IsNothing(e) Then prevItem = -1
                DeselectMenuItems(menuStackPanel, sender)
                tabDisplay.SelectedItem = reactionsPage
            Case Is = "inventory"
                DeselectMenuItems(menuStackPanel, sender)
                tabDisplay.SelectedItem = inventoryPage
            Case Is = "calculator"
                DeselectMenuItems(menuStackPanel, sender)
                tabDisplay.SelectedItem = calculatorTabPage
        End Select
    End Sub

    Private Sub selectTab(ByVal tag As String)
        For Each e As FrameworkElement In menuStackPanel.Children
            If Not IsNothing(e.Tag) AndAlso e.Tag.Equals(tag) Then
                handleMenuClick(e, Nothing)
                Exit For
            End If
        Next
    End Sub

    Private Sub createNewLab()
        Dim l As New Laboratory()
        Matter.Info.LoadLab(l)
    End Sub

    Private Sub aniCompleted(sender As Object, e As EventArgs) Handles da.Completed
        If hidden Then
            panelHider.SetValue(ImageButton.ImageBrushProperty, Me.FindResource("rightArrow"))
        Else
            leftPanel.InvalidateProperty(Grid.ActualWidthProperty)
            panelHider.SetValue(ImageButton.ImageBrushProperty, Me.FindResource("leftArrow"))
        End If
        animating = False
    End Sub

    Private Sub AnimateAllLabels(inElement As DependencyObject, ByVal hide As Boolean)
        For Each o As Object In LogicalTreeHelper.GetChildren(inElement)

            If o.GetType().IsEquivalentTo(GetType(Label)) Then
                Dim lb As Label = o

                Dim da As DoubleAnimation
                If hide Then
                    da = New DoubleAnimation(Me.FindResource("labelHeight"), 0, GetDuration(0.5))
                Else
                    da = New DoubleAnimation(Me.FindResource("labelHeight"), GetDuration(0.5))
                End If
                lb.BeginAnimation(Label.HeightProperty, da)
            Else
                If o.GetType().IsSubclassOf(GetType(DependencyObject)) AndAlso Not o.GetType().IsEquivalentTo(GetType(ImageButton)) Then
                    AnimateAllLabels(o, hide)
                End If
            End If
        Next
    End Sub

    Private Sub DeselectMenuItems(inElement As DependencyObject, ByVal exception As ImageButton)
        RecursiveDeselect(inElement, exception.Tag)
        exception.IsSelected = True
    End Sub

    Private Sub RecursiveDeselect(inElement As DependencyObject, ByVal exceptTag As String)
        For Each o As Object In LogicalTreeHelper.GetChildren(inElement)

            If o.GetType().IsEquivalentTo(GetType(ImageButton)) Then
                Dim tb As ImageButton = o
                If Not tb.Tag.Equals(exceptTag) Then tb.IsSelected = False
                If o.GetType().IsSubclassOf(GetType(DependencyObject)) Then
                    RecursiveDeselect(o, exceptTag)
                End If
            End If
        Next
    End Sub

#End Region

#Region "Searching"

    Private WithEvents timer1 As New Threading.DispatcherTimer
    Private Sub handleTick(sender As Object, e As EventArgs) Handles timer1.Tick
        searchBox.Text = searchBox.Template.FindName("tbCore", searchBox).Text
        timer1.Stop()
    End Sub

    Private lastSearch As String = String.Empty
    Private lastSearch2 As String = String.Empty
    Private lastSearch3 As String = String.Empty
    Private lastSearch4 As String = String.Empty
    Private Sub handleSearchChanged(sender As Object, e As TextChangedEventArgs)
        Dim tbCore As TextBox = sender
        Select Case tbCore.TemplatedParent.GetValue(TextBox.NameProperty)
            Case "searchBox"
                If Not lastSearch.Equals(sender.Text) Then
                    timer1.Stop()
                    timer1.Start()
                    lastSearch = sender.Text
                End If
            Case "organicSearchBox"
                If Not lastSearch2.Equals(sender.Text) Then
                    timer2.Stop()
                    timer2.Start()
                    lastSearch2 = sender.Text
                End If
            Case "inventorySearchBox"
                If Not lastSearch3.Equals(sender.Text) Then
                    timer3.Stop()
                    timer3.Start()
                    lastSearch3 = sender.Text
                End If
            Case "reactionsSearchBox"
                If Not lastSearch4.Equals(sender.Text) Then
                    timer4.Stop()
                    timer4.Start()
                    lastSearch4 = sender.Text
                End If
        End Select
    End Sub

    Private WithEvents timer2 As New Threading.DispatcherTimer
    Private Sub handleTick2(sender As Object, e As EventArgs) Handles timer2.Tick
        organicSearchBox.Text = organicSearchBox.Template.FindName("tbCore", organicSearchBox).Text
        timer2.Stop()
    End Sub

    Private WithEvents timer3 As New Threading.DispatcherTimer
    Private Sub handleTick3(sender As Object, e As EventArgs) Handles timer3.Tick
        inventorySearchBox.Text = inventorySearchBox.Template.FindName("tbCore", inventorySearchBox).Text
        timer3.Stop()
    End Sub

    Private WithEvents timer4 As New Threading.DispatcherTimer
    Private Sub handleTick4(sender As Object, e As EventArgs) Handles timer4.Tick
        reactionsSearchBox.Text = reactionsSearchBox.Template.FindName("tbCore", reactionsSearchBox).Text
        updateReactions()
        timer4.Stop()
    End Sub

#End Region

    Private Sub handleElementClick(sender As Object, e As RoutedEventArgs)
        Dim tb As TableButton = sender
        If Not IsNothing(tb.Element) Then
            Application.CreateElementInfoForm(tb.Element)
        End If
    End Sub

    Private Sub handleClosing(sender As Object, e As ComponentModel.CancelEventArgs)
        My.Settings.Save()
    End Sub

    Private Sub changeAutoLoadPath(sender As Object, e As MouseButtonEventArgs) Handles selectStartupLabel.MouseLeftButtonDown
        Dim o As New Microsoft.Win32.OpenFileDialog()
        o.Filter = OpenLabFilter
        o.Multiselect = False
        o.Title = "Select autoload laboratory"
        o.CheckFileExists = True
        o.CheckPathExists = True

        If o.ShowDialog(Me) Then
            My.Settings.AutoStartupFile = o.FileName
        End If
    End Sub

    Private Sub handleReactionTypeChange(sender As Object, e As SelectionChangedEventArgs) Handles reactionTypeComboBox.SelectionChanged
        If Me.IsInitialized Then updateReactions()
    End Sub

    Private Sub updateReactions()
        If reactionsSearchBox.Text.Length > 0 Then
            Dim rt As ReactionSearchConverter.SearchType = SearchType.All
            Select Case reactionTypeComboBox.SelectedIndex
                Case 1
                    rt = SearchType.Decomposition
                Case 2
                    rt = SearchType.Electrolysis
                Case 3
                    rt = SearchType.Synthesis
                Case 4
                    rt = SearchType.Other
            End Select
            Dim l As List(Of Matter.Reaction) = FindReactions(reactionsSearchBox.Text, rt)
            If l.Count > 0 Then
                If l.Count Mod 100 = 1 Then
                    reactionsStatusText.Content = String.Format("Found {0} reaction.", l.Count)
                Else
                    reactionsStatusText.Content = String.Format("Found {0} reactions.", l.Count)
                End If
            Else
                reactionsStatusText.Content = "No reactions found."
            End If
            reactionList.LoadReactions(l)
        Else
            reactionsStatusText.Content = "Search for a reaction. Use the negate symbol(-) for all reactions."
            reactionList.LoadReactions(New List(Of Matter.Reaction))
        End If
    End Sub

    Private Sub handleRowClicked(sender As Object, e As RoutedEventArgs) Handles reactionList.RowClicked
        Me.SelectedReaction = CType(sender, ReactionRow).Reaction
    End Sub

    Private WithEvents gridFadeAnimation As New DoubleAnimation(0, New Duration(TimeSpan.FromSeconds(1)))
    Private gridFadeInAnimation As New DoubleAnimation(1, New Duration(TimeSpan.FromSeconds(1)))
    Private isReactionSelected As Boolean = False
    Private Sub DeselectReaction()
        If isReactionSelected Then
            isReactionSelected = False
            For Each r As ReactantRow In reactantsPanel.Children
                RemoveHandler r.AmountChanged, amountChanged
            Next
            For Each r As ReactantRow In productsPanel.Children
                RemoveHandler r.AmountChanged, amountChanged
            Next
            searchReactionGrid.Visibility = Visibility.Visible
            selectedReactionGrid.BeginAnimation(Grid.OpacityProperty, gridFadeAnimation)
            searchReactionGrid.BeginAnimation(Grid.OpacityProperty, gridFadeInAnimation)
        End If
    End Sub

    Private prevItem As Integer = -1
    Private Sub SelectReaction(ByVal r As Matter.Reaction)
        If Not CType(tabDisplay.SelectedItem, TabItem).Name.Equals(reactionsPage.Name) Then
            prevItem = tabDisplay.SelectedIndex
            selectTab("reaction")
        End If

        isReactionSelected = True

        reactionTypeLabel.Content = r.Type.ToString()
        If r.IsCommented Then
            reactionCommentLabel.Content = r.Comment
        Else
            reactionCommentLabel.Content = "None"
        End If
        If r.HasTemperatureSpan Then
            reactionMinLabel.Content = r.TemperatureSpan.First() & " K"
            reactionMaxLabel.Content = r.TemperatureSpan.Last() & " K"
        Else
            reactionMinLabel.Content = "No limit"
            reactionMaxLabel.Content = "No limit"
        End If
        reactionSelectedRow.Reaction = r
        Dim whiteBrush As New SolidColorBrush(Color.FromArgb(15, 255, 255, 255))
        Dim blackBrush As New SolidColorBrush(Color.FromArgb(30, 0, 0, 0))
        reactantsPanel.Children.Clear()
        For i As Integer = 0 To r.Reactants.Count - 1
            Dim reactRow As New ReactantRow(r.Reactants(i), r.ReactantCoeficients(i))
            If i Mod 2 = 1 Then reactRow.Background = whiteBrush Else reactRow.Background = blackBrush
            AddHandler reactRow.AmountChanged, amountChanged
            reactantsPanel.Children.Add(reactRow)
        Next
        productsPanel.Children.Clear()
        For i As Integer = 0 To r.Products.Count - 1
            Dim proRow As New ReactantRow(r.Products(i), r.ProductCoeficients(i))
            If i Mod 2 = 1 Then proRow.Background = whiteBrush Else proRow.Background = blackBrush
            AddHandler proRow.AmountChanged, amountChanged
            productsPanel.Children.Add(proRow)
        Next
        If r.IsReversible Then
            arrowRect.Fill = Me.FindResource("reversibleReactionBrush")
        Else
            arrowRect.Fill = Me.FindResource("normalReactionBrush")
        End If

        If selectedReactionGrid.Opacity < 1 Then
            selectedReactionGrid.Visibility = Visibility.Visible
            searchReactionGrid.BeginAnimation(Grid.OpacityProperty, gridFadeAnimation)
            selectedReactionGrid.BeginAnimation(Grid.OpacityProperty, gridFadeInAnimation)
        End If
    End Sub

    Private Sub handleFadeOutCompleted(sender As Object, e As EventArgs) Handles gridFadeAnimation.Completed
        If isReactionSelected Then
            searchReactionGrid.Visibility = Visibility.Hidden
        Else
            selectedReactionGrid.Visibility = Visibility.Hidden
        End If
    End Sub

    Private amountChanged As New RoutedEventHandler(AddressOf amountChangedHandler)
    Private Sub amountChangedHandler(sender As Object, e As RoutedEventArgs)
        Dim sendingRow As ReactantRow = sender
        Dim moles As Decimal = Convert(sendingRow.Amount, sendingRow.SelectedUnit, Matter.UnitOfMass.Mole, New Matter.CompoundFormula(sendingRow.Formula))
        For Each r As ReactantRow In reactantsPanel.Children
            If Not r.Formula.Equals(sendingRow.Formula) Then
                r.Amount = Convert(getAmount(moles, selectedReaction_, sendingRow.Formula, r.Formula), Matter.UnitOfMass.Mole, r.SelectedUnit, New Matter.CompoundFormula(r.Formula))
            End If
        Next
        For Each r As ReactantRow In productsPanel.Children
            If Not r.Formula.Equals(sendingRow.Formula) Then
                r.Amount = Convert(getAmount(moles, selectedReaction_, sendingRow.Formula, r.Formula), Matter.UnitOfMass.Mole, r.SelectedUnit, New Matter.CompoundFormula(r.Formula))
            End If
        Next
    End Sub

    Private Shared Function getAmount(ByVal a As Decimal, ByVal r As Matter.Reaction, ByVal f1 As String, ByVal f2 As String) As Decimal
        Dim coef1 As Integer = 0
        If r.Reactants.Contains(f1) Then
            coef1 = r.ReactantCoeficients(r.Reactants.IndexOf(f1))
        Else
            coef1 = r.ProductCoeficients(r.Products.IndexOf(f1))
        End If
        Dim coef2 As Integer = 0
        If r.Reactants.Contains(f2) Then
            coef2 = r.ReactantCoeficients(r.Reactants.IndexOf(f2))
        Else
            coef2 = r.ProductCoeficients(r.Products.IndexOf(f2))
        End If
        Return a * coef2 / coef1
    End Function

    Private Sub backReactionClicked(sender As Object, e As RoutedEventArgs) Handles backToSearchBtn.Click
        If prevItem < 0 Then
            DeselectReaction()
        Else
            tabDisplay.SelectedIndex = prevItem
        End If
    End Sub

    Private Sub handleMolarityChange(sender As Object, e As RoutedEventArgs) Handles molarityNumberBox.NumberChanged
        If Me.IsInitialized AndAlso Not convertion1Toggle.Enabled Then updateNumbers()
    End Sub

    Private Sub handleMassChange(sender As Object, e As RoutedEventArgs) Handles massConcNumberBox.NumberChanged
        If Me.IsInitialized AndAlso convertion1Toggle.Enabled Then updateNumbers()
    End Sub

    Private Sub handleMolarMassChange(sender As Object, e As RoutedEventArgs) Handles molarMassNumberBox.NumberChanged
        updateNumbers()
    End Sub

    Private Sub handleDensityChange(sender As Object, e As RoutedEventArgs) Handles densityNumberBox.NumberChanged
        updateNumbers()
    End Sub

    Private Sub updateNumbers()
        If Not Me.IsInitialized Then Exit Sub
        If Not convertion1Toggle.Enabled Then
            If densityNumberBox.Number = 0 Then
                massConcNumberBox.Text = "Infinity"
            Else
                massConcNumberBox.Number = molarityNumberBox.Number * molarMassNumberBox.Number / (10 * densityNumberBox.Number)
            End If
        Else
            If molarMassNumberBox.Number = 0 Then
                molarityNumberBox.Text = "Infinity"
            Else
                molarityNumberBox.Number = massConcNumberBox.Number * (10 * densityNumberBox.Number) / molarMassNumberBox.Number
            End If
        End If
    End Sub

    Private Sub handleConv1Change(sender As Object, e As RoutedEventArgs) Handles convertion1Toggle.EnabledChanged
        If convertion1Toggle.Enabled Then
            arrowRectConv.RenderTransform = New RotateTransform(180)
        Else
            arrowRectConv.RenderTransform = Nothing
        End If
    End Sub

End Class
