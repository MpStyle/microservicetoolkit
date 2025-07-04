﻿using microservice.toolkit.messagemediator.extension;

namespace microservice.toolkit.messagemediator.utils;

public static class SignalHandlerUtils
{
    public static string PatternOf<T>() where T : ISignalHandler
    {
        return typeof(T).ToPattern();
    }
}