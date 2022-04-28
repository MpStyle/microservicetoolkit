import AppBar from '@material-ui/core/AppBar/AppBar';
import React, {FunctionComponent} from 'react';
import './Home.css';
import Typography from "@material-ui/core/Typography/Typography";
import {IconButton, Toolbar, useScrollTrigger} from "@material-ui/core";
import {useStyles} from "./Home.css";
import {Link} from "react-router-dom";
import Button from "@material-ui/core/Button";
import GitHubIcon from '@material-ui/icons/GitHub';

const Home: FunctionComponent = () => {
    const classes = useStyles();
    const trigger = useScrollTrigger();
    const gitHubUrl='https://github.com/MpStyle/microservicetoolkit';

    return <div className={classes.root}>
        <AppBar position="fixed"
                elevation={trigger ? undefined : 0}
                className={`${classes.appBar} ${trigger ? '' : classes.plainAppBar}`}>
            <Toolbar className={classes.toolbar}>
                <img src='logo-header.png'/>
                <Typography variant="h6" className={classes.title}>
                    MicroService Toolkit
                </Typography>
                <ul className={classes.navigation}>
                    <li><Link to='' className={classes.link}>Documentation</Link></li>
                    <li><Link to='' className={classes.link}>Contribute</Link></li>
                    <li>
                        <IconButton color={"primary"}
                                    href={gitHubUrl}>
                            <GitHubIcon/>
                        </IconButton>
                    </li>
                </ul>
            </Toolbar>
        </AppBar>

        <main>
            <div className={classes.mainWrapper}>
                <div className={classes.main}>
                    <Toolbar/>
                    <Typography variant="h3" className={classes.welcome}>
                        Welcome Mt!
                    </Typography>
                    <Typography variant="h6" className={classes.description}>
                        A progressive .NET and ASP.NET toolkit for coding fast, reliable and scalable server-side and
                        desktop applications.
                    </Typography>

                    <div className={classes.buttons}>
                        <Button variant="contained" color="primary" className={classes.button}>Documentation</Button>
                        <Button variant="contained"
                                color="secondary"
                                href={gitHubUrl}
                                className={classes.button}
                                startIcon={<GitHubIcon/>}>
                            Source code
                        </Button>
                    </div>
                </div>
            </div>
            <div className={classes.contentWrapper}>
                <div className={classes.content}>
                    <Typography variant="h4" className={classes.contentTitle}>
                        Contribute
                    </Typography>

                    <div className={classes.contributeContent}>
                        Microservice Toolkit is an MIT-licensed open-source project. It can grow also thanks to your
                        collaboration. Please, consider supporting us!
                    </div>
                </div>
            </div>
            <div className={classes.footerWrapper}>
                <div className={classes.footer}>
                    <IconButton color={"primary"}
                                href={gitHubUrl}>
                        <GitHubIcon/>
                    </IconButton>
                    <div>Released under the MIT License</div>
                    <div>Copyright © 2021- 2022 <a href="https://github.com/MpStyle">Michele Pagnin</a></div>
                    <div>Designed by <a href="https://github.com/MpStyle">Michele Pagnin</a>, hosted by GitHub</div>
                </div>
            </div>
        </main>
    </div>;
}

export default Home;
