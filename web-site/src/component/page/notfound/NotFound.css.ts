import createStyles from "@material-ui/core/styles/createStyles";
import { Theme } from "@material-ui/core/styles/createTheme";
import makeStyles from "@material-ui/core/styles/makeStyles";

export const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        notFound: {
            flexGrow: 1,
            height: '100%',
            width: '100%',
            background: 'url(images/leaves.jpg) no-repeat center center fixed',
            backgroundSize: 'cover',
            paddingTop: "80px",
        },
        mainWrapper: {
            paddingLeft: theme.spacing(2),
            paddingRight: theme.spacing(2),
            [theme.breakpoints.up('sm')]: {
                paddingLeft: theme.spacing(3),
                paddingRight: theme.spacing(3),
            },
            margin: "0px auto",
            paddingTop: theme.spacing() * 2,
            paddingBottom: theme.spacing() * 2,
            width: "90%",
            maxWidth: "750px",
            overflow: "hidden"
        },
        title: {
            textAlign: 'center'
        },
        contentWrapper: {
            textAlign: 'center',
            marginTop: '30px'
        },
        goHomeButton: {
            marginTop: '30px',
            marginBottom: '20px',
            display: 'inline-block'
        }
    })
);