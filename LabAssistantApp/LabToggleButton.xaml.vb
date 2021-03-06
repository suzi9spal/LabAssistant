﻿Imports System.ComponentModel
Imports System.Windows.Media.Animation

Public Class LabToggleButton
    Implements INotifyPropertyChanged

    Private Shared defaultEnabledBackBrush As Color = Color.FromArgb(48, 0, Colors.LimeGreen.G, Colors.LimeGreen.B)
    Private Shared defaultDisabledBackBrush As Color = Color.FromArgb(48, 255, 0, 0)
    Private Shared defaultEnabledBorderBrush As Color = Colors.Green
    Private Shared defaultDisabledBorderBrush As Color = Colors.Red

    Public Shared ReadOnly EnabledProperty As DependencyProperty =
        DependencyProperty.Register("Enabled", GetType(Boolean), GetType(LabToggleButton), New UIPropertyMetadata(False))
    Public Shared ReadOnly EnabledTextProperty As DependencyProperty =
        DependencyProperty.Register("EnabledText", GetType(String), GetType(LabToggleButton), New UIPropertyMetadata("ENABLED"))
    Public Shared ReadOnly DisabledTextProperty As DependencyProperty =
        DependencyProperty.Register("DisabledText", GetType(String), GetType(LabToggleButton), New UIPropertyMetadata("DISABLED"))

    Public Property EnabledBackgroundBrush As Color
        Get
            Return enBackCl
        End Get
        Set(value As Color)
            enBackCl = value
            updateBrushes()
            OnPropertyChanged("EnabledBackgroundBrush")
        End Set
    End Property
    Private enBackCl As Color = defaultEnabledBackBrush

    Public Property DisabledBackgroundBrush As Color
        Get
            Return diBackCl
        End Get
        Set(value As Color)
            diBackCl = value
            updateBrushes()
            OnPropertyChanged("DisabledBackgroundBrush")
        End Set
    End Property
    Private diBackCl As Color = defaultDisabledBackBrush

    Public Property EnabledBorderBrush As Color
        Get
            Return enBorderCl
        End Get
        Set(value As Color)
            enBorderCl = value
            updateBrushes()
            OnPropertyChanged("EnabledBorderBrush")
        End Set
    End Property
    Private enBorderCl As Color = defaultEnabledBorderBrush

    Public Property DisabledBorderBrush As Color
        Get
            Return diBorderCl
        End Get
        Set(value As Color)
            diBorderCl = value
            updateBrushes()
            OnPropertyChanged("DisabledBorderBrush")
        End Set
    End Property
    Private diBorderCl As Color = defaultDisabledBorderBrush

    Public Property EnabledText As String
        Get
            Return enTxt
        End Get
        Set(value As String)
            enTxt = value
            OnPropertyChanged("EnabledText")
        End Set
    End Property
    Private enTxt As String = "ENABLED"

    Public Property DisabledText As String
        Get
            Return disTxt
        End Get
        Set(value As String)
            disTxt = value
            OnPropertyChanged("DisabledText")
        End Set
    End Property
    Private disTxt As String = "DISABLED"

    Public Property Enabled As Boolean
        Get
            Return Me.GetValue(LabToggleButton.EnabledProperty)
        End Get
        Set(value As Boolean)
            If value Xor Me.GetValue(LabToggleButton.EnabledProperty) Then
                Me.SetValue(LabToggleButton.EnabledProperty, value)
                RaiseEnabledChangedEvent()
                updateBrushes()
            End If
        End Set
    End Property

    Private Sub updateBrushes()
        Dim ca As ColorAnimation
        Dim brdCa As ColorAnimation
        Dim ta As ThicknessAnimation
        Dim dur As New Duration(TimeSpan.FromSeconds(0.4))
        If Me.GetValue(LabToggleButton.EnabledProperty) Then
            ca = New ColorAnimation(enBackCl, dur)
            brdCa = New ColorAnimation(enBorderCl, dur)
            If started Then
                ta = New ThicknessAnimation(New Thickness(0), dur)
            Else
                ta = New ThicknessAnimation(New Thickness(0, 0, holdingGrid.ActualWidth - selectionBorder.ActualWidth, 0), dur)
            End If

        Else
            ca = New ColorAnimation(diBackCl, dur)
            brdCa = New ColorAnimation(diBorderCl, dur)
            If started Then
                ta = New ThicknessAnimation(New Thickness(holdingGrid.ActualWidth - selectionBorder.ActualWidth, 0, 0, 0), dur)
            Else
                ta = New ThicknessAnimation(New Thickness(0), dur)
            End If
        End If

        backBrush.BeginAnimation(SolidColorBrush.ColorProperty, ca)
        selBorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, brdCa)
        selectionBorder.BeginAnimation(Border.MarginProperty, ta)
    End Sub

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Dim eBind As New Binding("EnabledText")
        Dim dBind As New Binding("DisabledText")
        eBind.Source = Me
        dBind.Source = Me
        enableBtn.SetBinding(Button.ContentProperty, eBind)
        disableBtn.SetBinding(Button.ContentProperty, dBind)

    End Sub

    Private started As Boolean
    Public Sub handleLoad(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If Me.GetValue(LabToggleButton.EnabledProperty) Then
            started = True
            backBrush.Color = enBackCl
            selBorderBrush.Color = enBorderCl
            selectionBorder.HorizontalAlignment = HorizontalAlignment.Left
        Else
            started = False
            backBrush.Color = diBackCl
            selBorderBrush.Color = diBorderCl
            selectionBorder.HorizontalAlignment = HorizontalAlignment.Right
        End If
    End Sub

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Private Sub enableClicked(sender As Object, e As RoutedEventArgs) Handles enableBtn.Click
        Me.Enabled = True
    End Sub

    Private Sub disableClicked(sender As Object, e As RoutedEventArgs) Handles disableBtn.Click
        Me.Enabled = False
    End Sub

    Protected Overloads Sub OnPropertyChanged(propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    Protected Function SetField(Of T)(ByRef field As T, value As T, propertyName As String) As Boolean
        If EqualityComparer(Of T).[Default].Equals(field, value) Then
            Return False
        End If
        field = value
        OnPropertyChanged(propertyName)
        Return True
    End Function

    Public Shared ReadOnly EnabledChangedEvent As RoutedEvent = EventManager.RegisterRoutedEvent("EnabledChanged", RoutingStrategy.Bubble, GetType(RoutedEventHandler), GetType(LabToggleButton))

    <Browsable(True)>
    Public Custom Event EnabledChanged As RoutedEventHandler
        AddHandler(ByVal value As RoutedEventHandler)
            Me.AddHandler(EnabledChangedEvent, value)
        End AddHandler

        RemoveHandler(ByVal value As RoutedEventHandler)
            Me.RemoveHandler(EnabledChangedEvent, value)
        End RemoveHandler

        RaiseEvent(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Me.RaiseEvent(e)
        End RaiseEvent
    End Event

    Private Sub RaiseEnabledChangedEvent()
        Dim e As New RoutedEventArgs(LabToggleButton.EnabledChangedEvent)

        MyBase.RaiseEvent(e)
    End Sub

    Private Sub test(sender As Object, e As EventArgs) Handles Me.Initialized
        updateBrushes()
    End Sub

End Class
