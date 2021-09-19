module.exports = {
  apps : [{
    name: 'Apex API',
    script: 'dotnet',

    // Options reference: https://pm2.io/doc/en/runtime/reference/ecosystem-file/
    args: 'Apex_Api.dll --urls=http://0.0.0.0:54677',
    instances: 1,
    autorestart: true,
    watch: false,
    max_memory_restart: '1G',
    env: {
      NODE_ENV: 'development'
    },
    env_production: {
      NODE_ENV: 'production'
    }
  }],
};
