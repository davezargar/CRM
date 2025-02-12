import {NavLink} from "react-router";

//testar med textarea sålänge, finns förmodligen något bättre. Ska kika runt.
function CreateTicket()
{
    return <div><h1>Create Ticket</h1>
                <textarea name="form" id="formID" rows={4} cols={50}></textarea>

    <NavLink to="/"><button type="button">Return</button></NavLink>
    </div>

}
export default CreateTicket;