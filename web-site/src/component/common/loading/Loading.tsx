import CircularProgress from "@material-ui/core/CircularProgress/CircularProgress";
import createStyles from "@material-ui/core/styles/createStyles";
import makeStyles from "@material-ui/core/styles/makeStyles";
import { CreateCSSProperties } from "@material-ui/core/styles/withStyles";
import { CSSProperties } from "@material-ui/core/styles/withStyles";
import React, { FunctionComponent } from "react";

export const loadingStyle: CSSProperties | CreateCSSProperties<{}> = {
    margin: '0 auto',
    textAlign: 'center',
    maxWidth: '300px',

    '& .progress': {
        verticalAlign: 'middle',
        display: 'inline-block',
        marginRight: '15px'
    }
}

const useStyles = makeStyles(theme => createStyles({
    loading: loadingStyle,
}));

export const Loading: FunctionComponent<LoadingProps> = props => {
    const classes = useStyles();

    return <div className={props.className ?? classes.loading}>
        <span className="progress"><CircularProgress size={props.circularSize} /></span>
        <span className="message">{props.message}</span>
    </div>;
}

Loading.defaultProps = {
    message: 'Loading…'
};

export interface LoadingProps {
    message?: string;
    className?: string;
    circularSize?: number | string;
}