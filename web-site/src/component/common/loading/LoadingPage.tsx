import React, {FunctionComponent} from "react";
import {Theme} from "@material-ui/core/styles/createTheme";

export const LoadingPage: FunctionComponent<LoadingPageProps> = props => {
    return <div style={{
        background: 'url(images/icon/logo180.png) no-repeat center center',
        backgroundColor: props.theme.palette.background.default,
        height: '100%',
        width: '100%',
    }}/>;
}

export interface LoadingPageProps {
    theme: Theme;
}