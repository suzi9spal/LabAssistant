﻿Imports LabAssistantApp.Matter

Public Class ReactionRow
    Inherits ClickableControl

    Private displayConverter As New FormulaDisplayConverter()
    Public Sub New(r As Reaction)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        arrowPath.Stroke = Me.Foreground
        If r.IsReversible Then
            arrowPath.Data = Me.FindResource("reversibleReactionArrowAbsolute")
        Else
            arrowPath.Data = Me.FindResource("reactionArrowAbsolute")
        End If

        Dim reacList As New List(Of UIElement)
        For i As Integer = 0 To r.Reactants.Count - 1
            Dim cf As New CompoundFormula(r.Reactants.ElementAt(i))
            If i > 0 Then
                reacList.Add(getPlusGrid())
            End If
            Dim state As StateInLab = Chemical.GetLabStateFromFormula(cf.ToString())
            Dim coef As Integer = r.ReactantCoeficients.ElementAt(i)
            If Not (state = StateInLab.Unavailable) And coef > 1 Then
                Dim tb As New TextBlock()
                tb.Inlines.Add(coef)
                tb.VerticalAlignment = VerticalAlignment.Center
                tb.Foreground = Me.Foreground
                reacList.Add(tb)
                coef = -1
            End If
            reacList.Add(createLabel(coef, state, cf))
        Next

        Dim proList As New List(Of UIElement)
        For i As Integer = 0 To r.Products.Count - 1
            Dim cf As New CompoundFormula(r.Products.ElementAt(i))
            If i > 0 Then
                proList.Add(getPlusGrid())
            End If
            Dim state As StateInLab = Chemical.GetLabStateFromFormula(cf.ToString())
            Dim coef As Integer = r.ProductCoeficients.ElementAt(i)
            If Not (state = StateInLab.Unavailable) And coef > 1 Then
                Dim tb As New TextBlock()
                tb.Inlines.Add(coef)
                tb.VerticalAlignment = VerticalAlignment.Center
                tb.Foreground = Me.Foreground
                proList.Add(tb)
                coef = -1
            End If
            proList.Add(createLabel(coef, state, cf))
        Next

        For Each obj As UIElement In reacList
            reactantsPanel.Children.Add(obj)
        Next
        For Each obj As UIElement In proList
            productsPanel.Children.Add(obj)
        Next

        If r.Status = Reaction.ReactionStatus.Recreatable Then
            recreatablePath.Visibility = Visibility.Visible
        Else
            recreatablePath.Visibility = Visibility.Hidden
        End If
    End Sub

    Private Function getPlusGrid() As Grid
        Dim g As New Grid()
        g.RowDefinitions.Add(getRowDef)
        g.RowDefinitions.Add(getRowDef)
        g.RowDefinitions.Add(getRowDef)
        Dim plusPath As New Path()
        Grid.SetRow(plusPath, 1)
        plusPath.Stretch = Stretch.Uniform
        plusPath.Stroke = Me.Foreground
        plusPath.Data = Me.FindResource("reactionPlusAbsolute")
        g.Children.Add(plusPath)
        Return g
    End Function

    Private Function createLabel(ByVal coef As Integer, ByVal state As StateInLab, ByVal formula As CompoundFormula) As Label
        Dim l As New Label()
        If state = StateInLab.Unavailable Then
            l.Foreground = Me.Foreground
        Else
            l.Foreground = Me.FindResource(state.ToString())
        End If
        l.VerticalContentAlignment = VerticalAlignment.Center
        l.Content = displayConverter.Convert(formula, GetType(TextBlock), coef, Nothing)
        If coef < 0 Then l.Padding = New Thickness(0, 5, 5, 5)
        Return l
    End Function

    Private Shared Function getRowDef() As RowDefinition
        Dim r As New RowDefinition()
        r.Height = New GridLength(1, GridUnitType.Star)
        Return r
    End Function

End Class