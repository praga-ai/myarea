import React from 'react';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { MaterialIcons } from '@expo/vector-icons';
import DashboardScreen from '../screens/DashboardScreen';
import SurveyScreen from '../screens/SurveyScreen';
import MasterScreen from '../screens/MasterScreen';

export type RootTabParamList = {
  Dashboard: undefined;
  Survey: undefined;
  Master: undefined;
};

const Tab = createBottomTabNavigator<RootTabParamList>();

export default function RootNavigator() {
  return (
    <Tab.Navigator
      screenOptions={({ route }) => ({
        headerShown: false,
        tabBarIcon: ({ focused, color, size }) => {
          let iconName: keyof typeof MaterialIcons.glyphMap = 'dashboard';

          if (route.name === 'Dashboard') {
            iconName = 'dashboard';
          } else if (route.name === 'Survey') {
            iconName = 'assignment';
          } else if (route.name === 'Master') {
            iconName = 'storage';
          }

          return <MaterialIcons name={iconName} size={size} color={color} />;
        },
        tabBarActiveTintColor: '#2196F3',
        tabBarInactiveTintColor: '#999',
        tabBarLabelStyle: {
          fontSize: 12,
          marginBottom: 5,
        },
      })}
    >
      <Tab.Screen
        name="Dashboard"
        component={DashboardScreen}
        options={{ title: 'Dashboard' }}
      />
      <Tab.Screen
        name="Survey"
        component={SurveyScreen}
        options={{ title: 'Survey' }}
      />
      <Tab.Screen
        name="Master"
        component={MasterScreen}
        options={{ title: 'Master Data' }}
      />
    </Tab.Navigator>
  );
}
