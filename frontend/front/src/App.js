import React from "react";
import {BrowserRouter as Router, Routes, Route} from 'react-router-dom';
import Home from './pages/Home';
import axios from "axios";

class App extends React.Component{
  render(){
    return(
      <Router>
        <Routes>
          <Route path = "/" element={<Home />}/>
          {/* <Route path ="/login" element={<Login />} />
          <Route path ="/register" element={<Register />} /> */}

        </Routes>
      </Router>
    )
  }
}

export default App;