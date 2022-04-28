import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import {Routing} from "./component/Routing";

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);
root.render(
  <React.StrictMode>
    <Routing />
  </React.StrictMode>
);
