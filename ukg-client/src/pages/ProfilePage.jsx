import React from 'react';
import { useNavigate } from 'react-router-dom';  
import styled from 'styled-components'; 

import { useSelector } from 'react-redux';

const UserCard = styled.div`
  background-color: white;
  padding: 30px;
  border-radius: 10px;
  border-left: 6px solid rgb(10, 86, 60);
  box-shadow: 0 3px 10px rgba(0, 0, 0, 0.07);
  display: flex;
  flex-direction: column;
  justify-content: space-between; 

  span {
    font-size: 18px;
    color: #555;
    margin-bottom: 4px;
  }

  strong {
    color: #222;
  }
`;

const PageContainer = styled.div`
  display: flex;
  justify-content: center;
  align-items: center;
  flex-direction: column;
  height: 100vh;
  background-color: #f4f4f9;
  padding: 40px;
  box-sizing: border-box;
`;

 

const ProfileHeader = styled.div`
  font-size: 37px;
  font-weight: 800;
  color: #333; 
  width: 85%;
  text-align: center;  
`;

const UserNameHeader = styled.div`
  font-size: 37px;
  font-weight: 700;
  color: #333;
  margin-top: 11px;
  margin-bottom: 31px;
  text-align: center; 
`;  
  
const Button = styled.button`
  padding: 10px 20px;
  font-size: 16px;
  border-radius: 5px;
  border: none;
  cursor: pointer;
  transition: background-color 0.3s ease;
  width: 45%; 

  &:hover {
    opacity: 0.9;
  }
`; 

 
 
const ProfilePage = () => {  
  const user = useSelector((state) => state.auth.user);
  const navigate = useNavigate();    
  return (
    <PageContainer>
      {user?.id > 0 ?  
        < > 
          <ProfileHeader>{ user.roles ? user?.roles.join(" & ") : "Not Employee"}</ProfileHeader>
          <UserNameHeader>{ user.userName}</UserNameHeader>  
          <UserCard>  
            <span><strong>ID: </strong> { user.id}</span>
            <span><strong>Email: </strong> { user.email}</span>
            <span><strong>Username: </strong> { user.userName}</span>
            <span><strong>Phone: </strong> { user.phoneNumber}</span>
            <span><strong>Created: </strong> { new Date(user.dateCreated).toLocaleString()}</span>
            <span><strong>Your Roles: </strong>{ user.roles ? user?.roles.join(" & ") : "Not Employee"}</span>
          </UserCard> 
        </ > 
      : navigate('/signin') }
    </PageContainer>
  );
};

export default ProfilePage;
