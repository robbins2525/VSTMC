#pragma once

class App
{
public:
    static App& Instance();

    bool Initialize();
    void Shutdown();

private:
    App() = default;
    App(const App&) = delete;
    App& operator=(const App&) = delete;
};