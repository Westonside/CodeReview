import React, { useState } from "react";
import Header from "../components/Header";
import HomeBody from "../components/HomeBody";

const Home = () =>{
    const [search, setSearch] = useState("")
    const [logged, setLogged] = useState(true);
    return(
        <React.Fragment>
            <Header setSearched={setSearch} page={"index"}  />
            <HomeBody />
        </React.Fragment>
    )
}

export default Home;