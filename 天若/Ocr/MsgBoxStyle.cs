using System;

namespace TrOCR.Ocr;

[Flags]
public enum MsgBoxStyle
{
    OK,
    OKCancel,
    AbortRetryIgnore,
    YesNoCancel,
    YesNo,
    RetryCancel,
    CancelRetryContinue,
    RedCriticalOK = 16,
    RedCriticalOKCancel,
    RedCriticalAbortRetryIgnore,
    RedCriticalYesNoCancel,
    RedCriticalYesNo,
    RedCriticalRetryCancel,
    RedCriticalCancelRetryContinue,
    BlueQuestionOK = 32,
    BlueQuestionOKCancel,
    BlueQuestionAbortRetryIgnore,
    BlueQuestionYesNoCancel,
    BlueQuestionYesNo,
    BlueQuestionRetryCancel,
    BlueQuestionCancelRetryContinue,
    YellowAlertOK = 48,
    YellowAlertOKCancel,
    YellowAlertAbortRetryIgnore,
    YellowAlertYesNoCancel,
    YellowAlertYesNo,
    YellowAlertRetryCancel,
    YellowAlertCancelRetryContinue,
    BlueInfoOK = 64,
    BlueInfoOKCancel,
    BlueInfoAbortRetryIgnore,
    BlueInfoYesNoCancel,
    BlueInfoYesNo,
    BlueInfoRetryCancel,
    BlueInfoCancelRetryContinue
}
