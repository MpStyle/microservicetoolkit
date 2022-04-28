import Button from "@material-ui/core/Button";
import Paper from "@material-ui/core/Paper";
import Typography from "@material-ui/core/Typography/Typography";
import React, { FunctionComponent } from "react";
import { Link } from "react-router-dom";
import { useStyles } from "./NotFound.css";
import {AppPage} from "../../../book/AppPage";

export const NotFound: FunctionComponent = () => {
    const classes = useStyles();

    return <div className={classes.notFound}>
        <Paper className={classes.mainWrapper} elevation={6}>
            <Typography variant="h1" className={classes.title}>
                404
            </Typography>
            <Typography variant="h3" className={classes.title}>
                Page not found
            </Typography>

            <div className={classes.contentWrapper}>
                <div>
                    The page you are looking for was moved, removed, renamed or might never existed.
                </div>

                <Button variant="contained" color="primary" component={Link} to={AppPage.Home} className={classes.goHomeButton}>
                    Go home
                </Button>
            </div>

        </Paper>
    </div>;
}