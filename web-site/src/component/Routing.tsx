import {FunctionComponent, lazy, Suspense} from "react";
import {HashRouter, Route, Switch} from "react-router-dom";
import {AppPage} from "../book/AppPage";
import {appTheme} from "../book/theme/Theme";
import {createTheme, ThemeProvider} from '@material-ui/core/styles';
import CssBaseline from "@material-ui/core/CssBaseline/CssBaseline";
import {NotFound} from "./page/notfound/NotFound";
import {LoadingPage} from "./common/loading/LoadingPage";

export const Routing: FunctionComponent<RoutingProps> = props => {
    const theme = createTheme(appTheme);

    const Home = lazy(() => import("./page/home/Home"));

    return <ThemeProvider theme={theme}>
        <CssBaseline/>
        <HashRouter>
            <Suspense fallback={<LoadingPage theme={theme}/>}>
                <Switch>
                    <Route exact path={AppPage.Home} component={Home}/>
                    <Route component={NotFound}/>
                </Switch>
            </Suspense>
        </HashRouter>
    </ThemeProvider>;
};

export interface RoutingProps {
}